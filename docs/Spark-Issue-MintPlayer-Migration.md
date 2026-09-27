# Framework work needed by the MintPlayer migration: composable row policies & interceptors, SoftDelete + History packages, auth fixes, then a Moderation package

## Context

MintPlayer (music catalog, ~750 accounts) is being migrated from ASP.NET Core MVC + EF Core + SQL Server onto Spark + RavenDB and moved to the Hetzner VPS alongside CodeCoverage. The plan, decisions and spike results live in the MintPlayer repo: `docs/PRD-Spark-Completion.md` (decisions D1–D34, spikes S1–S10). This issue collects **everything the migration needs from Spark**, delivered in two PRs:

- **PR 1 — required for the MintPlayer cutover** (items 1–10). Merged and published first; MintPlayer consumes the release.
- **PR 2 — `MintPlayer.Spark.Moderation`** (item 11). Built on PR 1's packages afterwards; MintPlayer does not depend on it.

MintPlayer currently runs Spark `10.0.0-preview.41`; it will upgrade to the 11.x line (passkeys exist only there), so everything below targets master / 11.x.

---

## PR 1

### 1. Core seam: composable, DI-registered contributors (no base actions classes)

Today every per-type hook (`GetRowFilterAsync`, `IsAllowedAsync`, `OnBeforeSaveAsync`, `OnDeleteAsync`, …) is a single virtual on the actions class. A package that wants to add a filter or intercept a delete can only do so by shipping a base class — and an app cannot derive from two. Proposal:

- **Row policies** — `IRowPolicy` (or `IRowPolicy<T>`), registered in DI. Their filters are **AND-combined** with the actions class's own `GetRowFilterAsync`; their `IsAllowedAsync` results are AND-combined with the actions class's check. A policy declares which types it applies to, so it costs nothing for other types. Results cached per request (policies run on every list/detail/edit/delete/stream/breadcrumb load).
- **Lifecycle interceptors** — `IPersistentObjectInterceptor`: before/after save, before/after delete (**may replace the delete**, which is how soft delete works), after load. Run in registration order around the actions class hooks.
- Existing overrides keep working unchanged; the seam is additive.

Relates to #283 (breadcrumb loads bypassing the row gate) and #285 (row filter push-down) — whatever combination rule is chosen has to hold on those paths too.

### 2. `MintPlayer.Spark.SoftDelete` (new package, on the seam)

- Opt-in `ISoftDeletable` (`IsDeleted`, `DeletedAt`, `DeletedBy`).
- Interceptor turns `Delete` into "set the flag"; row policy hides deleted rows everywhere (list, detail, lookups, references, breadcrumbs).
- New rights `Restore/<Type>` and `Purge/<Type>` in `security.json`; holders of a "view deleted" right bypass the filter (restore UI needs to see the row).
- ng-spark: "Deleted" filter toggle + Restore/Purge actions for rights holders.

MintPlayer today does soft delete by overriding `OnLoadAsync(session,id)` / `OnQueryAsync(session)` — both removed in 5ebfaa45 — so this package is what replaces it during the upgrade.

### 3. `MintPlayer.Spark.History` (new package, on the seam)

- Enable RavenDB revisions **per entity from the model** (replaces app-side `ConfigureRevisionsOperation` code such as MintPlayer's `RevisionsConfigurator`).
- Audit stamping interceptor: `CreatedBy`/`ModifiedBy`/`DeletedBy` from the current user (opt-in interface).
- Endpoints: `GET /spark/po/{type}/{id}/revisions` (change vector, timestamp, `ModifiedBy`), `GET …/revisions/{cv}` (read-only PO), `POST …/revert/{cv}` (goes through the normal save pipeline, so row checks and stamping still apply). New `History/<Type>` and `Revert/<Type>` rights.
- ng-spark **History panel** on `spark-po-detail`: revision list, read-only view, field diff against the current version, Revert button.
- Should expose an event or hook when a revision is created — Moderation (PR 2) builds on it.

### 4. Auth: sign in by email (blocking for MintPlayer)

`POST /spark/auth/login` comes from `MapIdentityApi`, which looks the `email` field up **as a user name**. MintPlayer users sign in by email and 750 of 752 have a different user name, so every one of them would get 401 after migration. Resolve the identifier by email first, fall back to user name (applies to the 2FA and recovery-code steps too).

### 5. Auth: remaining secrets at rest

Recovery codes are hashed on master (`UserStore.HashRecoveryCode`). Still stored in plaintext: the authenticator key and external-login/OAuth tokens (`SetTokenAsync`, R2-M11 in the security audit). Encrypt or data-protect them.

### 6. Auth: account flows needed for parity (ng-spark-auth)

MintPlayer needs, and would rather build upstream than app-side:
- email confirmation page + a supported confirmation path — see #299;
- change-password and profile pages;
- 2FA enrollment + recovery-code regeneration;
- social login buttons for Google, Microsoft, Facebook, Twitter/X, LinkedIn + account linking/unlinking;
- passkey registration, listing, removal and passwordless sign-in (11.x `IUserPasskeyStore`);
- GDPR: personal-data export and account deletion hooks (the app supplies its own data; Spark supplies the account part).

### 7. Timezone for server-side rendering

`IRequestTimeZoneResolver` reads only the `X-Spark-Timezone` header and falls back to UTC. An SSR first request has no such header. Please also accept a cookie (name configurable), so the prerender pass can render local times; the client sets the cookie from `Intl.DateTimeFormat().resolvedOptions().timeZone`.

### 8. Security: `DisableActions` is not enforced server-side

`PersistentObject.DisableActions(...)` / `IClientAccessor.DisableActionsOn` only hide buttons; `ExecuteCustomAction` does not refuse a disabled action. Either enforce it or document loudly that row rules must be re-checked inside the action.

### 9. Build: unpinned `npm i` in `MintPlayer.Spark.Authorization`

The Authorization package's build runs `npm i @mintplayer/ng-spark-auth` without a version, which drifts other packages on a fresh clone / in CI (found while building MintPlayer's Docker image). Pin it, or let the consuming app own the dependency.

### 10. `MailManager` package — implements #432

Durable, templated mail for every Spark app (MintPlayer uses it for confirmation, reset, account-link and room mails):
- `IEmailSender<TUser>` / `ISparkLinkConfirmationSender` implementations that only **publish** a message `{template, language, to, data}` on Spark Messaging — nothing is lost when the app is redeployed or the SMTP relay restarts.
- A recipient resolves the **MJML** template, fills it with `data` (e.g. Scriban with strict variables), renders MJML → HTML (e.g. Mjml.Net) and sends over SMTP (plain internal hop to a Postfix container must work — no forced STARTTLS).
- Templates are **embedded resources of the consuming app** (`{name}.{lang}.mjml`), versioned with the code; a test helper renders every template × language with sample data so a missing field fails the build.
- Per-message-type retry/expiry, so e.g. a password-reset mail dead-letters before its token expires (defaults today: `MaxAttempts = 5`, `RetentionDays = 7`).
- Dev mode (the open questions in #432): in Development, mails go to a configurable developer address or a pickup folder instead of real recipients; templates that exist only in the repo are exactly what gets rendered, since they ship inside the app.

### Nice to have in PR 1

- **Custom actions returning data** — they return void today; MintPlayer worked around it with a separate endpoint.
- `MintPlayer.Dotnet.SocketExtensions`: publish a build matching the 11.x line (10.0.1 is the latest on NuGet).

---

## PR 2 — `MintPlayer.Spark.Moderation` (after PR 1 merges)

A generic, opt-in moderation platform modelled on **Stack Overflow**, built on the seam (1), SoftDelete (2) and History (3):

- **Votes & reputation:** up/down votes on any opt-in entity; reputation ledger (one document per event, summed by a map-reduce index); configurable reputation events.
- **Privileges:** thresholds declared in a `moderation.json` next to `security.json` (e.g. edit others' content, vote down, flag, review, delete, moderate); applied as groups through a custom `IGroupMembershipProvider` (request-scoped, cached).
- **Flags & review queues:** report any entity with a reason; review queue page; outcomes feed reputation.
- **Moderator tools:** lock/unlock, revert (History), restore/purge (SoftDelete), suspend user; every action written to an audit log.
- **New-account throttling** as a row policy.
- **ng-spark UI:** vote widget, flag button, review-queue page, reputation badge, moderator panel.
- **Out of v1:** suggested edits (held for review before going live) — they change save semantics.
- **Proof:** one of the Spark demo apps (`apps/DemoApp`, or a small new Q&A/wiki demo) uses the package end to end; its E2E tests are the spec. A design doc for PR 2 should be drafted **before PR 1 merges**, so the seam and the History/SoftDelete APIs are checked against it before they are published.

---

## Acceptance

- [ ] PR 1: seam (1), SoftDelete (2), History + History panel (3), email sign-in (4), secrets at rest (5), auth flows (6), timezone cookie (7), `DisableActions` (8), pinned npm (9), MailManager (10, closes #432) — published to nuget.org / npm.
- [ ] MintPlayer upgraded to that release (tracked in the MintPlayer repo).
- [ ] PR 2: Moderation package + demo app + E2E tests.
