import { ApplicationConfig, mergeApplicationConfig } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { appConfig } from './app.config';

const serverConfig: ApplicationConfig = {
  providers: [provideServerRendering()],
};

/** Browser config + server rendering. Request-scoped providers are added per render in main.server.ts. */
export const config = mergeApplicationConfig(appConfig, serverConfig);
