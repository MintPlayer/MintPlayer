/**
 * A single item in the play-queue. A flat, display-ready shape (the legacy `SongWithMedium | VideoUrl`
 * split collapses here): "which medium to play" is resolved at enqueue time, so the player only ever
 * sees a `url`. Used as the `TVideo` of the `PlaylistController` queue engine.
 *
 * NB: the engine clones entries (`Object.assign`) and matches on object identity of its own clones,
 * so consumers compare/track by {@link key}, not object reference — see `PlayerService` and PRD §4.2.
 */
export interface PlaylistEntry {
  /** Stable identity for track-by + dedupe (a song's id-derived key, or the url for ad-hoc entries). */
  readonly key: string;
  /** The media URL handed to `<video-player>`. Must be `findApis`-playable. */
  readonly url: string;
  /** Display label in the sidebar (song title / breadcrumb, or the raw url). */
  readonly title: string;
  /** Optional deep-link to the catalog detail page (songs); absent for ad-hoc urls. */
  readonly routerLink?: string[];
}
