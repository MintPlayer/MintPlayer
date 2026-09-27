#!/usr/bin/env bash
# Warm-path TTFB for prerendered pages vs. the plain CSR shell and the JSON API.
B=http://localhost:5101
measure() { # label url n
  local label=$1 url=$2 n=$3
  for i in $(seq 1 "$n"); do curl -s -o /dev/null -w "%{time_starttransfer}\n" "$url"; done \
    | sort -n | awk -v l="$label" '{a[NR]=$1} END {printf "%-28s n=%d min=%.1fms p50=%.1fms p90=%.1fms max=%.1fms\n", l, NR, a[1]*1000, a[int(NR*0.5)+1]*1000, a[int(NR*0.9)]*1000, a[NR]*1000}'
}
measure "SSR /song/313 (same)" "$B/song/313" 50
# distinct songs (no per-id caching anywhere)
for id in 312 311 310 309 308 307 306 305 304 303 302 301 300 299 298 297 296 295 294 293; do
  curl -s -o /dev/null -w "%{time_starttransfer}\n" "$B/song/$id"
done | sort -n | awk '{a[NR]=$1} END {printf "%-28s n=%d min=%.1fms p50=%.1fms max=%.1fms\n", "SSR /song/{20 ids}", NR, a[1]*1000, a[int(NR*0.5)+1]*1000, a[NR]*1000}'
measure "CSR shell /songs (skipped)" "$B/query/songs" 50
measure "API /api/public/song/313" "$B/api/public/song/313" 50
