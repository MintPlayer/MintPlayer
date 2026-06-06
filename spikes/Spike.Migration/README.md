# Spike 0.4 — SQL Server → RavenDB migration + auth round-trip

Throwaway Phase-0 spike (per `docs/Implementation-Plan-Spark-Migration.md`). It de-risks the
three trickiest migration decisions against the **real** MintPlayer.Spark stack (the local clone
at `C:\Repos\MintPlayer.Spark`) running on an embedded RavenDB instance.

## What it proves

| Test file | Decision | Claim proven |
|-----------|----------|--------------|
| `PasswordHashPortabilityTests` | **D8** — migrate password hashes verbatim, no reset | An Identity-V3 (PBKDF2) hash produced by the old app authenticates through Spark's real `UserManager` + RavenDB `UserStore`. A lower-iteration *legacy* hash still verifies **and** is reported `SuccessRehashNeeded`, so Identity transparently upgrades it on next login (forward-compatible). |
| `TwoFactorContinuityTests` | **2FA continuity** | A TOTP code from an **independent** RFC-6238 generator (standing in for the user's phone app) validates through Identity's `AuthenticatorTokenProvider` against the verbatim-copied `AuthenticatorKey`. Existing enrollments keep working — no re-enrollment. |
| `ReferenceResolutionTests` | **Structural migration / R5** | The polymorphic SQL `Subject` TPH table splits into `Artist`/`Person`/`Song` collections; join-table flags (`ArtistPerson.Active`, `ArtistSong.Credited`) survive as embedded arrays; cross-collection `[Reference]` string IDs resolve; the Tag self-tree resolves; the `TagCategory` ARGB colour round-trips; deterministic IDs (`artists/1`, `people/3`…) make the ETL idempotent. |

## Layout

- `Source/SqlSource.cs` — POCO mirror + hand-seeded snapshot of the current SQL/EF rows (the real
  Phase-7 tool reads these from `MintPlayer.Data` via EF; the spike hand-seeds them).
- `Domain/SparkModel.cs` — the target RavenDB/Spark domain (deterministic IDs, embedded join arrays, `[Reference]`).
- `Etl/Migrator.cs` — maps source → domain and writes to RavenDB.
- `Totp/Authenticator.cs` — independent RFC-6238 TOTP generator (the "phone app").
- `AuthSpikeBase.cs` — builds a real `UserManager<SparkUser>` over the embedded store.

## Running

```pwsh
dotnet test spikes/Spike.Migration/Spike.Migration.csproj
```

**Prerequisite:** RavenDB 7.x requires a license even for the embedded test driver. Provide it via
the `RAVENDB_LICENSE` env var **or** a `raven-license.log` file at the repo root (already present
locally and git-ignored — never commit it). Without it, `SparkTestDriver` fails fast with a clear
message.

Status: **5/5 passing.** Once the findings are folded into the real `MintPlayer.Migration` tool
(Phase 7), this spike can be deleted.
