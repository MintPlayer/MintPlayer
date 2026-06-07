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
import { ColorColumnRenderer } from './renderers/color-column-renderer';
import { ColorDetailRenderer } from './renderers/color-detail-renderer';
import { ColorEditRenderer } from './renderers/color-edit-renderer';

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
    ]),
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
