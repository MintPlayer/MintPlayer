// Dev catalog seeder: pulls the live MintPlayer v1 API and writes the catalog into the local
// RavenDB (MintPlayer db) as Spark domain documents with deterministic ids — idempotent (re-runnable).
//
//   node scripts/seed-catalog.mjs
//
// Not part of the app; a one-off local convenience to get realistic data for the player/playlist UI.
// The real SQL->RavenDB migration is the Phase 7 MintPlayer.Migration tool.

const API = 'https://mintplayer.com/api/v1';
const RAVEN = 'http://localhost:8080';
const DB = 'MintPlayer';
const CREATED_AT = '2026-06-09T00:00:00.0000000+00:00'; // fixed → re-runs don't churn the field

const clr = (t) => `MintPlayer.Domain.Entities.${t}, MintPlayer.Domain`;
const dateOnly = (s) => (s ? String(s).slice(0, 10) : null); // "1975-10-31T00:00:00" -> "1975-10-31"
const audit = (oldId) => ({ CreatedAt: CREATED_AT, ModifiedAt: null, IsDeleted: false, DeletedAt: null, OldId: oldId });
const mapTags = (tags) => (tags ?? []).filter((t) => t?.id != null).map((t) => `Tags/${t.id}`);

async function getList(ep) {
  const res = await fetch(`${API}/${ep}`, { headers: { Accept: 'application/json', include_relations: 'true' } });
  if (!res.ok) throw new Error(`${ep} -> HTTP ${res.status}`);
  return res.json();
}

// live media [{id,type:{id,description},value}] -> Spark Medium [{Value,TypeId}] (+ collect MediumTypes)
function mapMedia(media, playerInfos, mediumTypes) {
  const items = Array.isArray(media) && media.length ? media : null;
  if (items) {
    return items.filter((m) => m?.value).map((m) => {
      let typeId = null;
      if (m.type?.id != null) {
        typeId = `MediumTypes/${m.type.id}`;
        if (!mediumTypes.has(m.type.id))
          mediumTypes.set(m.type.id, { Name: m.type.description ?? `Type ${m.type.id}`, Description: null, ...audit(m.type.id) });
      }
      return { Value: m.value, TypeId: typeId };
    });
  }
  // fall back to the computed player URLs (no medium-type info)
  return (playerInfos ?? []).filter((p) => p?.url).map((p) => ({ Value: p.url, TypeId: null }));
}

const main = async () => {
  console.log('Fetching live API (artists, songs, people, tags)…');
  const [artists, songs, persons, tags] = await Promise.all([
    getList('artist'), getList('song'), getList('person'), getList('tag'),
  ]);
  console.log(`  artists=${artists.length} songs=${songs.length} persons=${persons.length} tags=${tags.length}`);

  const mediumTypes = new Map();   // liveId -> doc
  const tagCategories = new Map(); // liveId -> doc (collected from each tag's category)
  const people = new Map();        // liveId -> doc  (from the person list + artist members, deduped)
  const docs = [];                 // { id, coll, type, body }

  // Tags + their categories (subjects reference these via TagIds).
  for (const t of tags) {
    if (t.category?.id != null && !tagCategories.has(t.category.id)) {
      tagCategories.set(t.category.id, {
        Description: t.category.description ?? '', Color: t.category.color ?? '#000000', ...audit(t.category.id),
      });
    }
    docs.push({ id: `Tags/${t.id}`, coll: 'Tags', type: clr('Tag'), body: {
      Description: t.description ?? '',
      CategoryId: t.category?.id != null ? `TagCategories/${t.category.id}` : null,
      ParentId: t.parent?.id != null ? `Tags/${t.parent.id}` : null,
      ...audit(t.id),
    }});
  }

  const addPerson = (p) => {
    if (!p || p.id == null || people.has(p.id)) return;
    people.set(p.id, {
      FirstName: p.firstName ?? '', LastName: p.lastName ?? '',
      Born: dateOnly(p.born), Died: dateOnly(p.died),
      Media: mapMedia(p.media, p.playerInfos, mediumTypes), TagIds: mapTags(p.tags), ...audit(p.id),
    });
  };

  persons.forEach(addPerson);

  for (const a of artists) {
    (a.currentMembers ?? []).forEach(addPerson);
    (a.pastMembers ?? []).forEach(addPerson);
    const members = [
      ...(a.currentMembers ?? []).map((p) => ({ PersonId: `People/${p.id}`, Active: true })),
      ...(a.pastMembers ?? []).map((p) => ({ PersonId: `People/${p.id}`, Active: false })),
    ];
    docs.push({ id: `Artists/${a.id}`, coll: 'Artists', type: clr('Artist'), body: {
      Name: a.name ?? '', YearStarted: a.yearStarted ?? null, YearQuit: a.yearQuit ?? null,
      Members: members, Media: mapMedia(a.media, a.playerInfos, mediumTypes), TagIds: mapTags(a.tags), ...audit(a.id),
    }});
  }

  for (const s of songs) {
    const credits = [
      ...(s.artists ?? []).map((a) => ({ ArtistId: `Artists/${a.id}`, Credited: true })),
      ...(s.uncreditedArtists ?? []).map((a) => ({ ArtistId: `Artists/${a.id}`, Credited: false })),
    ];
    docs.push({ id: `Songs/${s.id}`, coll: 'Songs', type: clr('Song'), body: {
      Title: s.title ?? '', Released: dateOnly(s.released),
      Artists: credits, Media: mapMedia(s.media, s.playerInfos, mediumTypes), TagIds: mapTags(s.tags), ...audit(s.id),
    }});
  }

  for (const [id, p] of people) docs.push({ id: `People/${id}`, coll: 'People', type: clr('Person'), body: p });
  for (const [id, mt] of mediumTypes) docs.push({ id: `MediumTypes/${id}`, coll: 'MediumTypes', type: clr('MediumType'), body: mt });
  for (const [id, tc] of tagCategories) docs.push({ id: `TagCategories/${id}`, coll: 'TagCategories', type: clr('TagCategory'), body: tc });

  const commands = docs.map((d) => ({
    Id: d.id, ChangeVector: null, Type: 'PUT',
    Document: { ...d.body, '@metadata': { '@collection': d.coll, 'Raven-Clr-Type': d.type } },
  }));

  console.log(`Writing ${commands.length} docs (people=${people.size}, artists=${artists.length}, songs=${songs.length}, tags=${tags.length}, tagCategories=${tagCategories.size}, mediumTypes=${mediumTypes.size})…`);
  const res = await fetch(`${RAVEN}/databases/${DB}/bulk_docs`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ Commands: commands }),
  });
  if (!res.ok) { console.error('bulk_docs failed', res.status, await res.text()); process.exit(1); }
  const out = await res.json();
  console.log(`Done — ${out.Results?.length ?? 0} documents written.`);
};

main().catch((e) => { console.error(e); process.exit(1); });
