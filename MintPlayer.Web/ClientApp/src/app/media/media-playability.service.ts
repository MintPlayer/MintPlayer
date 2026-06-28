import { Injectable } from '@angular/core';
import { IApiService } from '@mintplayer/player-provider';
import { findApis } from '@mintplayer/video-player';
import { VIDEO_PLAYER_PLUGINS } from './video-player-plugins';

/**
 * Decides whether a media URL can be played by the <video-player>, by delegating to the video-player
 * package's own URL detection (`findApis`) rather than reimplementing provider regexes. The platform
 * api services are resolved once and cached — only their `urlRegexes` are used here, so the per-platform
 * `loadApi()` (the external script load) never runs; the check is cheap enough to run per grid row.
 */
@Injectable({ providedIn: 'root' })
export class MediaPlayabilityService {
  private readonly apis: Promise<IApiService[]> = Promise.all(VIDEO_PLAYER_PLUGINS.map((plugin) => plugin()));

  /** True when some registered platform recognises this URL (i.e. the player can play it). */
  async canPlay(url: string | null | undefined): Promise<boolean> {
    if (!url) {
      return false;
    }
    const apis = await this.apis;
    return findApis(url, apis).length > 0;
  }
}
