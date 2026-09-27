"""S7 auth probe: JWT login on api/v1 for a *synthetic* user, then the authorized endpoints.

Usage:  python apiv1_auth_probe.py --base http://localhost:5401 --raven http://localhost:8080 --database MintPlayer_S7

1. registers a synthetic user through Spark's Identity API (/spark/auth/register) with a random password that
   never leaves this process, then marks its e-mail confirmed and gives it a user name that differs from the
   e-mail (like 750 of 752 production users) straight in RavenDB;
2. seeds a private playlist (Playlists/900001) owned by it and a like of Songs/43;
3. logs in on POST api/v1/account/login by e-mail and by user name, and calls current-user, roles, playlist/my,
   the private playlist, subject likes - with and without the token.

It refuses any database whose name does not end in "_S7" (the migrated snapshot holds production accounts).
Never prints tokens or passwords. Exit code 0 = all checks pass.
"""
import argparse, base64, json, secrets, sys, urllib.error, urllib.request

EMAIL = 's7-synthetic@example.invalid'
USERNAME = 's7-synthetic-user'
PLAYLIST_ID = 'Playlists/900001'

def call(method, url, body=None, headers=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header('Accept', 'application/json')
    if data is not None: req.add_header('Content-Type', 'application/json')
    for k, v in (headers or {}).items(): req.add_header(k, v)
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            raw = r.read()
            return r.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as e:
        raw = e.read()
        try: return e.code, (json.loads(raw) if raw else None)
        except ValueError: return e.code, None

def rql(raven, db, query, params):
    status, body = call('PATCH', f'{raven}/databases/{db}/queries?waitForIndexesTimeout=00:00:15',
                        {'Query': {'Query': query, 'QueryParameters': params}})
    if status >= 300: raise RuntimeError(f'RQL patch failed: {status}')
    op = body.get('OperationId')
    for _ in range(60):
        s, st = call('GET', f'{raven}/databases/{db}/operations/state?id={op}')
        if st and st.get('Status') in ('Completed', 'Faulted', 'Canceled'):
            if st['Status'] != 'Completed': raise RuntimeError(f'RQL patch {st["Status"]}')
            return
        import time; time.sleep(0.25)
    raise RuntimeError('RQL patch timed out')

def identity_v3_hash(password, prf='sha256', iterations=10_000):
    """ASP.NET Identity PasswordHasher V3: 0x01 | prf | iterations | salt length | salt | 32-byte subkey.
    The default (HMAC-SHA256, 10 000 iterations) is the legacy format of 183 production hashes (S3)."""
    import hashlib, struct
    salt = secrets.token_bytes(16)
    subkey = hashlib.pbkdf2_hmac(prf, password.encode(), salt, iterations, 32)
    code = {'sha1': 0, 'sha256': 1, 'sha512': 2}[prf]
    return base64.b64encode(bytes([1]) + struct.pack('>III', code, iterations, 16) + salt + subkey).decode()

def stored_hash_format(raven, db):
    q = {'Query': 'from MintPlayerUsers where Email = $e select PasswordHash', 'QueryParameters': {'e': EMAIL}}
    s, body = call('POST', f'{raven}/databases/{db}/queries', q)
    import struct
    raw = base64.b64decode(body['Results'][0]['PasswordHash'])
    return struct.unpack('>II', raw[1:9])  # (prf, iterations)

def put_doc(raven, db, doc_id, doc):
    status, _ = call('PUT', f'{raven}/databases/{db}/docs?id={urllib.request.quote(doc_id)}', doc)
    if status >= 300: raise RuntimeError(f'PUT {doc_id} failed: {status}')

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--base', default='http://localhost:5401')
    ap.add_argument('--raven', default='http://localhost:8080')
    ap.add_argument('--database', default='MintPlayer_S7')
    a = ap.parse_args()
    if not a.database.endswith('_S7'):
        print('refusing: the probe only writes to a *_S7 copy'); return 2
    v1 = f'{a.base}/api/v1'
    password = 'S7!' + secrets.token_urlsafe(18) + 'aA1'
    results = []
    def check(name, ok, detail=''):
        results.append(ok); print(f'{"PASS" if ok else "FAIL"}  {name}{("  - " + detail) if detail else ""}')

    # 1. user
    status, _ = call('POST', f'{a.base}/spark/auth/register', {'email': EMAIL, 'password': password})
    check('synthetic user exists (registered now or by an earlier run)', status in (200, 204, 400), f'register status {status}')
    # Make it look migrated: user name != e-mail, confirmed, and a *legacy-format* hash (V3 HMAC-SHA256 / 10k),
    # so the login below exercises the verbatim-migrated-hash path (D8) including the rehash on success.
    rql(a.raven, a.database,
        'from MintPlayerUsers where Email = $e update { this.EmailConfirmed = true; this.UserName = $u; this.NormalizedUserName = $nu; '
        'this.PasswordHash = $h; this.AccessFailedCount = 0; this.LockoutEnd = null; this.Roles = []; }',
        {'e': EMAIL, 'u': USERNAME, 'nu': USERNAME.upper(), 'h': identity_v3_hash(password)})
    check('stored hash is the legacy format before login', stored_hash_format(a.raven, a.database) == (1, 10_000))

    # 2. login by e-mail, by user name, wrong password
    s, login = call('POST', f'{v1}/account/login', {'email': EMAIL, 'password': password})
    token = (login or {}).get('token')
    check('login by e-mail -> 200 + token', s == 200 and bool(token) and login.get('status') == 1, f'status {s}')
    check('legacy hash rehashed to V3/SHA-512/100k by the login', stored_hash_format(a.raven, a.database) == (2, 100_000))
    check('login result shape', login is not None and set(login) >= {'status', 'user', 'error', 'errorDescription', 'token'}
          and set(login['user']) == {'id', 'userName', 'email', 'isTwoFactorEnabled', 'bypass2faForExternalLogin', 'pictureUrl'})
    s2, login2 = call('POST', f'{v1}/account/login', {'email': USERNAME, 'password': password})
    check('login by user name -> 200 + token', s2 == 200 and bool((login2 or {}).get('token')), f'status {s2}')
    s3, _ = call('POST', f'{v1}/account/login', {'email': EMAIL, 'password': password + 'x'})
    check('wrong password -> 401', s3 == 401, f'status {s3}')
    if not token:
        return 1
    claims = json.loads(base64.urlsafe_b64decode(token.split('.')[1] + '=='))
    check('token claims: nameid/unique_name/email/iss/aud/exp', {'nameid', 'unique_name', 'email', 'iss', 'aud', 'exp'} <= set(claims),
          ', '.join(sorted(claims)))
    user_id = claims['nameid']
    auth = {'Authorization': f'Bearer {token}'}

    # 3. seed: private playlist + a like
    put_doc(a.raven, a.database, PLAYLIST_ID, {
        'Name': 'S7 synthetic private playlist', 'Description': None, 'IsPublic': False, 'OwnerId': user_id,
        'Tracks': [{'SongId': 'Songs/23'}, {'SongId': 'Songs/6'}], 'CreatedAt': '2026-09-27T00:00:00.0000000+00:00',
        'ModifiedAt': None, 'IsDeleted': False, 'DeletedAt': None, 'OldId': 900001,
        '@metadata': {'@collection': 'Playlists', 'Raven-Clr-Type': 'MintPlayer.Domain.Entities.Playlist, MintPlayer.Domain'}})
    put_doc(a.raven, a.database, f'UserLikes/{user_id}', {
        'UserId': user_id, 'Likes': ['Songs/43'], 'Dislikes': [],
        '@metadata': {'@collection': 'UserLikes', 'Raven-Clr-Type': 'MintPlayer.Domain.Entities.UserLike, MintPlayer.Domain'}})

    # 4. authorized endpoints
    s, me = call('GET', f'{v1}/account/current-user', headers=auth)
    check('current-user with token -> 200, own record', s == 200 and me and me.get('email') == EMAIL and me.get('userName') == USERNAME, f'status {s}')
    s, _ = call('GET', f'{v1}/account/current-user')
    check('current-user without token -> 401', s == 401, f'status {s}')
    s, _ = call('GET', f'{v1}/account/current-user', headers={'Authorization': 'Bearer ' + token[:-4] + 'AAAA'})
    check('current-user with a tampered token -> 401', s == 401, f'status {s}')
    s, roles = call('GET', f'{v1}/account/roles', headers=auth)
    check('roles with token -> 200, []', s == 200 and roles == [], f'status {s}, {roles}')
    s, mine = call('GET', f'{v1}/playlist/my', headers={**auth, 'include_relations': 'true'})
    check('playlist/my with token -> the private playlist with 2 tracks',
          s == 200 and isinstance(mine, list) and [p['id'] for p in mine] == [900001] and len(mine[0]['tracks']) == 2
          and mine[0]['accessibility'] == 0 and mine[0]['user']['userName'] == USERNAME, f'status {s}')
    s, _ = call('GET', f'{v1}/playlist/900001')
    check('private playlist anonymous -> 401', s == 401, f'status {s}')
    s, pl = call('GET', f'{v1}/playlist/900001', headers=auth)
    check('private playlist as owner -> 200', s == 200 and pl and pl['id'] == 900001, f'status {s}')
    s, likes = call('GET', f'{v1}/subject/43/likes', headers=auth)
    check('subject/43/likes with token -> like = true, authenticated', s == 200 and likes and likes['like'] is True and likes['authenticated'] is True, f'{likes}')
    s, likes = call('GET', f'{v1}/subject/43/likes')
    check('subject/43/likes anonymous -> like = null', s == 200 and likes and likes['like'] is None and likes['authenticated'] is False, f'{likes}')

    # 5. hidden medium types (D15): only a privileged caller sees them
    s, anon_types = call('GET', f'{v1}/mediumtype')
    s_user, _ = call('GET', f'{v1}/mediumtype/15', headers=auth)
    check('hidden medium type 15 as a plain user -> 404', s_user == 404, f'status {s_user}')
    s, songs_anon = call('GET', f'{v1}/song', headers={'include_relations': 'true'})
    rql(a.raven, a.database, 'from MintPlayerUsers where Email = $e update { this.Roles = ["Administrator"]; }', {'e': EMAIL})
    try:
        s, t15 = call('GET', f'{v1}/mediumtype/15', headers=auth)
        check('hidden medium type 15 as Administrator -> 200, visible = false', s == 200 and t15 and t15['visible'] is False, f'status {s}')
        s, admin_types = call('GET', f'{v1}/mediumtype', headers=auth)
        check('medium type list: Administrator sees the hidden types too', s == 200 and len(admin_types) > len(anon_types),
              f'{len(anon_types)} anonymous vs {len(admin_types or [])} admin')
        s, songs_admin = call('GET', f'{v1}/song', headers={**auth, 'include_relations': 'true'})
        count = lambda songs: sum(len(x['media'] or []) for x in songs)
        hidden = sum(1 for x in songs_anon for m in (x['media'] or []) if not m['type']['visible'])
        check('song media: anonymous gets no hidden-type media; Administrator gets more', hidden == 0 and count(songs_admin) > count(songs_anon),
              f'{count(songs_anon)} anonymous vs {count(songs_admin)} admin')
    finally:
        rql(a.raven, a.database, 'from MintPlayerUsers where Email = $e update { this.Roles = []; }', {'e': EMAIL})

    # 6. a 2FA account gets status 2 (RequiresTwoFactor) and no token - v1 has no second step (legacy behaviour)
    rql(a.raven, a.database, 'from MintPlayerUsers where Email = $e update { this.TwoFactorEnabled = true; }', {'e': EMAIL})
    try:
        s, tf = call('POST', f'{v1}/account/login', {'email': EMAIL, 'password': password})
        check('2FA account -> 200, status 2, no token', s == 200 and tf and tf['status'] == 2 and tf['token'] is None, f'status {s}')
    finally:
        rql(a.raven, a.database, 'from MintPlayerUsers where Email = $e update { this.TwoFactorEnabled = false; }', {'e': EMAIL})
    return 0 if all(results) else 1

if __name__ == '__main__':
    sys.exit(main())
