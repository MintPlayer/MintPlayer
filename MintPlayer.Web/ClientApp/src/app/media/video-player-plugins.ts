import { ApiPlugin } from '@mintplayer/player-provider';
import { youtubePlugin } from '@mintplayer/youtube-player';
import { vimeoPlugin } from '@mintplayer/vimeo-player';
import { dailymotionPlugin } from '@mintplayer/dailymotion-player';
import { soundCloudPlugin } from '@mintplayer/soundcloud-player';
import { spotifyPlugin } from '@mintplayer/spotify-player';
import { filePlugin } from '@mintplayer/file-player';

/**
 * The platforms MintPlayer media can be played on. Single source of truth shared by:
 *  - `provideVideoApis(...)` (the actual <video-player> wiring), and
 *  - `MediaPlayabilityService` (the "is this URL playable?" check),
 * so the button only appears for URLs the player can really handle. Add a platform by importing its
 * `@mintplayer/{platform}-player` plugin and listing it here.
 */
export const VIDEO_PLAYER_PLUGINS: ApiPlugin[] = [
  youtubePlugin,
  vimeoPlugin,
  dailymotionPlugin,
  soundCloudPlugin,
  spotifyPlugin,
  filePlugin,
];
