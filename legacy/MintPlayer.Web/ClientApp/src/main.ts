import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';

import { environment } from './environments/environment';
import { AppBrowserModule } from './app/app.browser.module';
import { BootFuncParams, BOOT_FUNC_PARAMS } from '@mintplayer/ng-base-url';


if (!environment.production) {
  console.log('Development');
}

const providers: StaticProvider[] = [
  { provide: 'MINTPLAYER_API_VERSION', useValue: 'v3' },

  { provide: SERVER_SIDE, useValue: false },
  { provide: BOOT_FUNC_PARAMS, useValue: <BootFuncParams>null },

  { provide: 'PEOPLE', useValue: null },
  { provide: 'PERSON', useValue: null },
  { provide: 'ARTISTS', useValue: null },
  { provide: 'ARTIST', useValue: null },
  { provide: 'SONGS', useValue: null },
  { provide: 'SONG', useValue: null },
  { provide: 'MEDIUMTYPES', useValue: null },
  { provide: 'MEDIUMTYPE', useValue: null },
  { provide: 'TAGCATEGORIES', useValue: null },
  { provide: 'TAGCATEGORY', useValue: null },
  { provide: 'TAG', useValue: null },
  { provide: 'PLAYLIST', useValue: null },
  { provide: 'PLAYLISTS', useValue: null },
  { provide: 'BLOGPOSTS', useValue: null },
  { provide: 'BLOGPOST', useValue: null },
  { provide: 'LOGINS', useValue: null },
  { provide: 'PROVIDERS', useValue: null },
  { provide: 'USER', useValue: null },
  { provide: 'PATH', useValue: null },
  { provide: 'URL', useValue: null },
];

if (environment.production) {
  enableProdMode();
}

document.addEventListener('DOMContentLoaded', () => {
  platformBrowserDynamic(providers).bootstrapModule(AppBrowserModule)
  .catch(err => console.error(err));
});
