import 'zone.js/dist/zone-node';
import 'reflect-metadata';
import { renderModule, renderModuleFactory } from '@angular/platform-server';
import { APP_BASE_HREF } from '@angular/common';
import { enableProdMode, StaticProvider } from '@angular/core';
import { provideModuleMap } from '@nguniversal/module-map-ngfactory-loader';
import { createServerRenderer } from 'aspnet-prerendering';
import { environment } from './environments/environment';
export { AppServerModule } from './app/app.server.module';

if (environment.production) {
  enableProdMode();
}

export default createServerRenderer(params => {
  const { AppServerModule, AppServerModuleNgFactory, LAZY_MODULE_MAP } = (module as any).exports;

  const providers: StaticProvider[] = [
    provideModuleMap(LAZY_MODULE_MAP),
    { provide: APP_BASE_HREF, useValue: params.baseUrl },
    { provide: 'BASE_URL', useValue: params.origin + params.baseUrl }
  ];

  if ('people' in params.data) {
    providers.push({ provide: 'PEOPLE', useValue: params.data.people })
  }
  if ('person' in params.data) {
    providers.push({ provide: 'PERSON', useValue: params.data.person })
  }
  if ('artists' in params.data) {
    providers.push({ provide: 'ARTISTS', useValue: params.data.artists })
  }
  if ('artist' in params.data) {
    providers.push({ provide: 'ARTIST', useValue: params.data.artist })
  }
  if ('songs' in params.data) {
    providers.push({ provide: 'SONGS', useValue: params.data.songs })
  }
  if ('song' in params.data) {
    providers.push({ provide: 'SONG', useValue: params.data.song })
  }
  if ('mediumType' in params.data) {
    providers.push({ provide: 'MEDIUMTYPE', useValue: params.data.mediumType })
  }
  if ('mediumTypes' in params.data) {
    providers.push({ provide: 'MEDIUMTYPES', useValue: params.data.mediumTypes })
  }
  if ('lang' in params.data) {
    providers.push({ provide: 'LANG', useValue: params.data.lang });
  }

  const options = {
    document: params.data.originalHtml,
    url: params.url,
    extraProviders: providers
  };

  // Bypass ssr api call cert warnings in development
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

  const renderPromise = AppServerModuleNgFactory
    ? /* AoT */ renderModuleFactory(AppServerModuleNgFactory, options)
    : /* dev */ renderModule(AppServerModule, options);

  return renderPromise.then(html => ({ html }));
});
