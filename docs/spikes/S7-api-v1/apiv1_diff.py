"""S7 diff harness: replay recorded legacy api/v1 responses against the new app, field by field.

Usage:  python apiv1_diff.py [--base http://localhost:5401] [--refs <dir> ...] [--verbose]   (refs default: ./recorded)

Each case names a recorded legacy response (JSON file, looked up in the --refs directories), the request that
produced it and the expected status. The new app's response is compared recursively (objects by key, arrays by
index). A difference is *explained* when a DELTAS rule matches its path; every rule is a documented delta in
RESULT.md. Exit code 0 = no unexplained difference, 1 = unexplained differences, 2 = harness error.

Synthetic references (no live call was spent on them) are built from other recordings by DERIVED functions.
"""
import argparse, json, math, os, re, sys, urllib.request, urllib.error

# (name, method, path, include_relations, ref file or None, expected status, body or None)
CASES = [
    ('artist list',            'GET', 'artist', False, 'artists.json', 200, None),
    ('artist list rel',        'GET', 'artist', True, 'artists_rel.json', 200, None),
    ('artist 22',              'GET', 'artist/22', False, 'artist_22.json', 200, None),
    ('artist 22 rel',          'GET', 'artist/22', True, 'artist_22_rel.json', 200, None),
    ('person list',            'GET', 'person', False, 'people.json', 200, None),
    ('person list rel',        'GET', 'person', True, 'people_rel.json', 200, None),
    ('person 170',             'GET', 'person/170', False, 'person_170.json', 200, None),
    ('person 7 rel',           'GET', 'person/7', True, 'person_7_rel.json', 200, None),
    ('song list',              'GET', 'song', False, 'songs.json', 200, None),
    ('song list rel',          'GET', 'song', True, 'songs_rel.json', 200, None),
    ('song 23',                'GET', 'song/23', False, 'song_23.json', 200, None),
    ('song 23 rel',            'GET', 'song/23', True, 'song_23_rel.json', 200, None),
    ('song 6 rel',             'GET', 'song/6', True, 'song_6_rel.json', 200, None),
    ('song 99999 (missing)',   'GET', 'song/99999', False, None, 404, None),
    ('song 6 lyrics',          'GET', 'song/6/lyrics', False, 'lyrics6.json', 200, None),
    ('song 23 lyrics (timed)', 'GET', 'song/23/lyrics', False, 'lyrics_23.json', 200, None),
    ('song page (derived)',    'POST', 'song/page', False, 'DERIVED:song_page', 200,
        {'page': 2, 'perPage': 25, 'sortProperty': 'Id', 'sortDirection': 1}),
    ('mediumtype list',        'GET', 'mediumtype', False, 'mediumtype.json', 200, None),
    ('mediumtype 1',           'GET', 'mediumtype/1', False, 'mediumtype_1.json', 200, None),
    ('mediumtype 5 (missing)', 'GET', 'mediumtype/5', False, None, 404, None),
    ('tag list',               'GET', 'tag', False, 'tag.json', 200, None),
    ('tag list rel',           'GET', 'tag', True, 'tag_rel.json', 200, None),
    ('tag 2',                  'GET', 'tag/2', False, 'tag_2.json', 200, None),
    ('tag 2 rel',              'GET', 'tag/2', True, 'tag_2_rel.json', 200, None),
    ('tagcategory list',       'GET', 'tagcategory', False, 'tagcategory.json', 200, None),
    ('tagcategory list rel',   'GET', 'tagcategory', True, 'tagcategory_rel.json', 200, None),
    ('tagcategory 1 rel',      'GET', 'tagcategory/1', True, 'tagcategory_1_rel.json', 200, None),
    ('playlist public',        'GET', 'playlist/public', False, 'playlist_public.json', 200, None),
    ('playlist public rel',    'GET', 'playlist/public', True, 'pl_public.json', 200, None),
    ('playlist 9 rel',         'GET', 'playlist/9', True, 'playlist_9_rel.json', 200, None),
    ('playlist 16',            'GET', 'playlist/16', False, 'playlist_16.json', 200, None),
    ('playlist 1 (deleted)',   'GET', 'playlist/1', False, None, 404, None),
    ('blogpost list',          'GET', 'blogpost', False, 'blog.json', 200, None),
    ('blogpost 1',             'GET', 'blogpost/1', False, 'blogpost_1.json', 200, None),
    ('subject 6 likes',        'GET', 'subject/6/likes', False, 'likes6.json', 200, None),
    ('subject 23 likes',       'GET', 'subject/23/likes', False, 'likes_23.json', 200, None),
    ('subject 22 likes',       'GET', 'subject/22/likes', False, 'likes_22.json', 200, None),
    ('current-user anonymous', 'GET', 'account/current-user', False, None, 401, None),
    ('playlist my anonymous',  'GET', 'playlist/my', False, None, 401, None),
]

# Documented deltas (RESULT.md "Deltas"): (id, path regex, reason). Paths look like "[3#12].songs[0#40].description" (index#legacy id).
# An optional 4th element is a predicate (legacy_value, new_value) -> bool that must also hold.
DELTAS = [
    ('D1', r'(^|\.)concurrencyStamp$', 'SQL rowversion not migrated; now the RavenDB change vector (opaque string)'),
    ('D2', r'(^|\.)media\[\d+(#\d+)?\]\.id$', 'legacy Media.Id not migrated (media are embedded); always 0'),
    ('D3', r'(^|\.)dateUpdate$', 'legacy DateInsert 0001-01-01 without DateUpdate: the migration stores the snapshot as-of instant (S5 created-fallback)',
        lambda legacy, new: legacy == '0001-01-01T00:00:00'),
    ('D4', r'(^|\.)media$', 'media with an empty value are not migrated (S5: 2 empty media skipped)',
        lambda legacy, new: isinstance(legacy, list) and isinstance(new, list)
            and [m for m in legacy if m.get('value')] == legacy[:len(new)] and all(not m.get('value') for m in legacy[len(new):])),
    ('D5', r'#291\]\.lyrics\.timeline$', 'Songs/291: legacy showed its latest (untimed) lyrics row; D12 migrates the most complete timeline (S6)'),
]

def derived_song_page(refs, case):
    body = case[6]
    songs = load_ref(refs, 'songs.json')
    items = sorted(songs, key=lambda s: s['id'])
    if body['sortDirection'] == 1: items.reverse()
    page = items[(body['page'] - 1) * body['perPage']: body['page'] * body['perPage']]
    data = []
    for s in page:
        s = dict(s)
        s.update(playerInfos=[], youtubeId=None, dailymotionId=None, vimeoId=None, soundCloudUrl=None, description=s['title'])
        data.append(s)
    return {'data': data, 'page': body['page'], 'perPage': body['perPage'], 'totalRecords': len(songs),
            'totalPages': math.ceil(len(songs) / body['perPage'])}

DERIVED = {'song_page': derived_song_page}

def load_ref(refs, name):
    for d in refs:
        p = os.path.join(d, name)
        if os.path.exists(p):
            with open(p, encoding='utf-8-sig') as f:
                return json.load(f)
    raise FileNotFoundError(name)

def request(base, method, path, rel, body):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(f'{base}/api/v1/{path}', data=data, method=method)
    req.add_header('Accept', 'application/json')
    req.add_header('include_relations', 'true' if rel else 'false')
    if data is not None: req.add_header('Content-Type', 'application/json')
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            raw = r.read()
            return r.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as e:
        return e.code, None

def numeq(a, b):
    return isinstance(a, (int, float)) and isinstance(b, (int, float)) and not isinstance(a, bool) and not isinstance(b, bool) and abs(a - b) < 1e-9

def diff(expected, actual, path, out):
    if isinstance(expected, dict) and isinstance(actual, dict):
        for k in expected:
            p = f'{path}.{k}' if path else k
            if k not in actual: out.append((p, 'missing field', expected[k], None))
            else: diff(expected[k], actual[k], p, out)
        for k in actual:
            if k not in expected: out.append((f'{path}.{k}' if path else k, 'extra field', None, actual[k]))
    elif isinstance(expected, list) and isinstance(actual, list):
        if len(expected) != len(actual): out.append((path, f'length {len(expected)} vs {len(actual)}', expected, actual))
        for i in range(min(len(expected), len(actual))):
            tag = f'#{expected[i]["id"]}' if isinstance(expected[i], dict) and 'id' in expected[i] else ''
            diff(expected[i], actual[i], f'{path}[{i}{tag}]', out)
    elif expected != actual and not numeq(expected, actual):
        out.append((path, 'value', expected, actual))

def short(v):
    s = json.dumps(v, ensure_ascii=False)
    return s if len(s) <= 90 else s[:87] + '...'

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--base', default='http://localhost:5401')
    ap.add_argument('--refs', action='append', help='directories with recorded responses (default: ./recorded)')
    ap.add_argument('--verbose', action='store_true')
    a = ap.parse_args()
    a.refs = a.refs or [os.path.join(os.path.dirname(os.path.abspath(__file__)), 'recorded')]
    rules = [(i, re.compile(rx), why, (x[0] if x else (lambda e, a: True))) for i, rx, why, *x in DELTAS]
    total_unexplained, explained = 0, {}
    for case in CASES:
        name, method, path, rel, ref, status, body = case
        got_status, got = request(a.base, method, path, rel, body)
        if got_status != status:
            print(f'FAIL  {name}: status {got_status}, expected {status}')
            total_unexplained += 1
            continue
        if ref is None:
            print(f'PASS  {name}: status {status}')
            continue
        expected = DERIVED[ref[8:]](a.refs, case) if ref.startswith('DERIVED:') else load_ref(a.refs, ref)
        diffs = []
        diff(expected, got, '', diffs)
        unexplained = []
        for d in diffs:
            rule = next((r for r in rules if r[1].search(d[0]) and r[3](d[2], d[3])), None)
            if rule: explained[rule[0]] = explained.get(rule[0], 0) + 1
            else: unexplained.append(d)
        total_unexplained += len(unexplained)
        print(f'{"PASS" if not unexplained else "FAIL"}  {name}: {len(diffs)} differences, {len(unexplained)} unexplained')
        for d in unexplained[: (10**6 if a.verbose else 12)]:
            print(f'        {d[0]}: {d[1]}  legacy={short(d[2])}  new={short(d[3])}')
    print()
    print('Explained differences per documented delta:')
    for i, _, why, *_x in DELTAS:
        print(f'  {i:4} {explained.get(i, 0):6}  {why}')
    print(f'\nUNEXPLAINED: {total_unexplained}')
    return 0 if total_unexplained == 0 else 1

if __name__ == '__main__':
    try:
        sys.exit(main())
    except Exception as e:  # noqa: BLE001
        print(f'HARNESS ERROR: {e!r}')
        sys.exit(2)
