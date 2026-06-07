import { ApplicationConfig, provideBrowserGlobalErrorListeners, isDevMode } from '@angular/core';
import { provideRouter, TitleStrategy } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideSparkAuth, withSparkAuth } from '@mintplayer/ng-spark-auth';
import { provideSparkClientOperations } from '@mintplayer/ng-spark/client-operations';
import { provideSparkAttributeRenderers } from '@mintplayer/ng-spark/renderers';

import { routes } from './app.routes';
import { provideServiceWorker } from '@angular/service-worker';
import { MintPlayerTitleStrategy } from './seo/title-strategy';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // Spark auth interceptor + XSRF (X-XSRF-TOKEN) on the same-origin /spark API.
    provideHttpClient(...withSparkAuth()),
    provideAnimations(),
    provideSparkAuth(),
    provideSparkClientOperations(),
    // Custom attribute renderers (tag picker, karaoke editor, video player, …) get
    // registered here as the catalog UI is built out (Phase 2+).
    provideSparkAttributeRenderers([]),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
    // SEO base: route titles become "<page> | MintPlayer".
    { provide: TitleStrategy, useClass: MintPlayerTitleStrategy },
  ],
};
