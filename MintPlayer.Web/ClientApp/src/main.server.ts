import 'zone.js/node';
import 'reflect-metadata';
import { renderModule } from '@angular/platform-server';
import { APP_BASE_HREF } from '@angular/common';
import { enableProdMode, StaticProvider } from '@angular/core';
import { createServerRenderer } from 'aspnet-prerendering';
import { AppServerModule } from './app/app.server.module';
export { AppServerModule } from './app/app.server.module';

enableProdMode();

export default createServerRenderer(params => {
  const providers: StaticProvider[] = [
    { provide: APP_BASE_HREF, useValue: params.baseUrl },
    { provide: 'API_VERSION', useValue: 'v3' },
  ];

  //#region Provide data passed from C#
  if ('people' in params.data) {
    providers.push({ provide: 'PEOPLE', useValue: params.data.people });
  } else {
    providers.push({ provide: 'PEOPLE', useValue: null });
  }
  if ('person' in params.data) {
    providers.push({ provide: 'PERSON', useValue: params.data.person });
  } else {
    providers.push({ provide: 'PERSON', useValue: null });
  }
  if ('artists' in params.data) {
    providers.push({ provide: 'ARTISTS', useValue: params.data.artists });
  } else {
    providers.push({ provide: 'ARTISTS', useValue: null });
  }
  if ('artist' in params.data) {
    providers.push({ provide: 'ARTIST', useValue: params.data.artist });
  } else {
    providers.push({ provide: 'ARTIST', useValue: null });
  }
  if ('songs' in params.data) {
    providers.push({ provide: 'SONGS', useValue: params.data.songs });
  } else {
    providers.push({ provide: 'SONGS', useValue: null });
  }
  if ('song' in params.data) {
    providers.push({ provide: 'SONG', useValue: params.data.song });
  } else {
    providers.push({ provide: 'SONG', useValue: null });
  }
  if ('mediumtypes' in params.data) {
    providers.push({ provide: 'MEDIUMTYPES', useValue: params.data.mediumtypes });
  } else {
    providers.push({ provide: 'MEDIUMTYPES', useValue: null });
  }
  if ('mediumtype' in params.data) {
    providers.push({ provide: 'MEDIUMTYPE', useValue: params.data.mediumtype });
  } else {
    providers.push({ provide: 'MEDIUMTYPE', useValue: null });
  }
  if ('logins' in params.data) {
    providers.push({ provide: 'LOGINS', useValue: params.data.logins });
  } else {
    providers.push({ provide: 'LOGINS', useValue: null });
  }
  if ('providers' in params.data) {
    providers.push({ provide: 'PROVIDERS', useValue: params.data.providers });
  } else {
    providers.push({ provide: 'PROVIDERS', useValue: null });
  }
  if ('tagcategories' in params.data) {
    providers.push({ provide: 'TAGCATEGORIES', useValue: params.data.tagcategories });
  } else {
    providers.push({ provide: 'TAGCATEGORIES', useValue: null });
  }
  if ('tagcategory' in params.data) {
    providers.push({ provide: 'TAGCATEGORY', useValue: params.data.tagcategory });
  } else {
    providers.push({ provide: 'TAGCATEGORY', useValue: null });
  }
  if ('tag' in params.data) {
    providers.push({ provide: 'TAG', useValue: params.data.tag });
  } else {
    providers.push({ provide: 'TAG', useValue: null });
  }
  if ('playlist' in params.data) {
    providers.push({ provide: 'PLAYLIST', useValue: params.data.playlist });
  } else {
    providers.push({ provide: 'PLAYLIST', useValue: null });
  }
  if ('playlists' in params.data) {
    providers.push({ provide: 'PLAYLISTS', useValue: params.data.playlists });
  } else {
    providers.push({ provide: 'PLAYLISTS', useValue: null });
  }
  if ('blogposts' in params.data) {
    providers.push({ provide: 'BLOGPOSTS', useValue: params.data.blogposts });
  } else {
    providers.push({ provide: 'BLOGPOSTS', useValue: null });
  }
  if ('blogpost' in params.data) {
    providers.push({ provide: 'BLOGPOST', useValue: params.data.blogpost });
  } else {
    providers.push({ provide: 'BLOGPOST', useValue: null });
  }
  if ('user' in params.data) {
    providers.push({ provide: 'USER', useValue: params.data.user });
  } else {
    providers.push({ provide: 'USER', useValue: null });
  }
  if ('path' in params.data) {
    providers.push({ provide: 'PATH', useValue: params.data.path });
  } else {
    providers.push({ provide: 'PATH', useValue: null });
  }
  providers.push({ provide: 'URL', useValue: params.url });
  //#endregion

  const options = {
    document: params.data.originalHtml,
    url: params.url,
    extraProviders: providers
  };

  // Bypass ssr api call cert warnings in development
  process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = "0";

  const renderPromise = renderModule(AppServerModule, options);

  return renderPromise.then(html => ({ html }));
});
