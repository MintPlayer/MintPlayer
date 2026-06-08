import { ApplicationConfig, provideBrowserGlobalErrorListeners, isDevMode } from '@angular/core';
import { provideRouter, TitleStrategy } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideSparkAuth, withSparkAuth } from '@mintplayer/ng-spark-auth';
import { provideSparkClientOperations } from '@mintplayer/ng-spark/client-operations';
import { provideSparkAttributeRenderers } from '@mintplayer/ng-spark/renderers';

import { routes } from './app.routes';
import { provideServiceWorker } from '@angular/service-worker';
import { provideBaseHref } from '@mintplayer/ng-base-url';
import { MintPlayerTitleStrategy } from './seo/title-strategy';
import { provideVideoApis } from '@mintplayer/ng-video-player';
import { ColorColumnRenderer } from './renderers/color-column-renderer';
import { ColorDetailRenderer } from './renderers/color-detail-renderer';
import { ColorEditRenderer } from './renderers/color-edit-renderer';
import { MediaColumnRenderer } from './renderers/media-column-renderer';
import { MediaDetailRenderer } from './renderers/media-detail-renderer';
import { VIDEO_PLAYER_PLUGINS } from './media/video-player-plugins';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // Spark auth interceptor + XSRF (X-XSRF-TOKEN) on the same-origin /spark API.
    provideHttpClient(...withSparkAuth()),
    provideAnimations(),
    provideSparkAuth(),
    provideSparkClientOperations(),
    // Custom attribute renderers. Bind to a model attribute via its "renderer" field
    // (e.g. TagCategory.Color → "color-swatch"). More land here as the catalog UI grows.
    provideSparkAttributeRenderers([
      {
        name: 'color-swatch',
        columnComponent: ColorColumnRenderer,
        detailComponent: ColorDetailRenderer,
        editComponent: ColorEditRenderer,
      },
      {
        // Medium.Value → play-triangle button (when the URL is playable) + the URL.
        // No editComponent: the URL stays a normal text input on the edit form.
        name: 'media-player',
        columnComponent: MediaColumnRenderer,
        detailComponent: MediaDetailRenderer,
      },
    ]),
    // Register the video platforms so <video-player> (and the playability check) can handle them.
    provideVideoApis(...VIDEO_PLAYER_PLUGINS),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
    // SEO base: route titles become "<page> | MintPlayer".
    { provide: TitleStrategy, useClass: MintPlayerTitleStrategy },
    // @mintplayer/ng-seo: APP_BASE_HREF for canonical/href-lang URL building.
    provideBaseHref(),
  ],
};
