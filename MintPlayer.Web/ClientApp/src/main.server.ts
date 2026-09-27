// Server boot module for MintPlayer.AspNetCore.SpaServices.Prerendering (UseSpaPrerendering in
// Program.cs). Built by the application builder's `server` entry into dist/ClientApp/server/main.server.mjs;
// the library's prerenderer.js `import()`s it and calls the default export once per request.
import { mergeApplicationConfig } from '@angular/core';
import { bootstrapApplication, BootstrapContext } from '@angular/platform-browser';
import { renderApplication } from '@angular/platform-server';
import { provideBaseHref } from '@mintplayer/ng-base-url';
import { App } from './app/app';
import { config } from './app/app.config.server';
import { PRERENDER_DATA } from './app/ssr/prerender-data';
import { createServerRenderer } from './app/ssr/server-renderer';

export default createServerRenderer(async (params) => {
  const { originalHtml, ...data } = params.data;

  // Request-scoped providers at the application level (they must override app.config's providers,
  // which platform-level providers would not).
  const requestConfig = mergeApplicationConfig(config, {
    providers: [
      { provide: PRERENDER_DATA, useValue: data },
      // ng-base-url's no-arg provideBaseHref() (app.config) has no BOOT_FUNC_PARAMS dependency and
      // throws on the server; passing the params supplies the request origin for og:url/canonical.
      provideBaseHref(params),
    ],
  });

  const html = await renderApplication(
    (context: BootstrapContext) => bootstrapApplication(App, requestConfig, context),
    {
      document: originalHtml,
      // Path + query only. An absolute URL would trip platform-server 22's `allowedHosts` SSRF guard
      // (it would need the request Host echoed back as allowed, which defeats the guard). Public pages
      // get their data from OnSupplyData, so no relative HttpClient call is made during the render.
      url: params.url,
    },
  );
  return { html };
});
