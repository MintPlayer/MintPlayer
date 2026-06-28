import { PlaylistEntry } from './playlist-entry';

/**
 * Builds a {@link PlaylistEntry} from a single media URL. The URL is assumed already playable
 * (callers gate on {@link MediaPlayabilityService.canPlay} before enqueuing). The `key` defaults
 * to the URL so identical URLs dedupe naturally; pass an explicit `key` (e.g. a song id) when a
 * stable catalog identity is available.
 */
export function playlistEntryFromUrl(
  url: string,
  opts?: { key?: string; title?: string; routerLink?: string[] },
): PlaylistEntry {
  return {
    key: opts?.key ?? url,
    url,
    title: opts?.title?.trim() || url,
    routerLink: opts?.routerLink,
  };
}
