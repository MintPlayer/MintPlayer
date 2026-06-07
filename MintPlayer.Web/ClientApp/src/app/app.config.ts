import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideSparkAuth, withSparkAuth } from '@mintplayer/ng-spark-auth';
import { provideSparkClientOperations } from '@mintplayer/ng-spark/client-operations';
import { provideSparkAttributeRenderers } from '@mintplayer/ng-spark/renderers';

import { routes } from './app.routes';

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
  ],
};
