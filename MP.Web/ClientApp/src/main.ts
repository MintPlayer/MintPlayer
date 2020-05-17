import { enableProdMode, StaticProvider, Inject, InjectionToken } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { environment } from './environments/environment';
import { AppBrowserModule } from './app/app.browser.module';

const getBaseUrl = () => {
  return document.getElementsByTagName('base')[0].href.slice(0, -1);
}
const getExternalUrl = (baseUrl: string) => {
  if (new RegExp("\\blocalhost\\b").test(baseUrl)) {
    return baseUrl;
  } else {
    let match = new RegExp("^(http[s]{0,1})\\:\\/\\/(.*)$").exec(baseUrl);

    let protocol = match[1];
    let url = match[2];

    return `${protocol}://external.${url}`;
  }
}


const providers: StaticProvider[] = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'EXTERNAL_URL', useFactory: getExternalUrl, deps: ['BASE_URL'] },
  { provide: 'SERVERSIDE', useValue: false },
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
  { provide: 'URL', useValue: null }
];

if (environment.production) {
  enableProdMode();
}

document.addEventListener('DOMContentLoaded', () => {
  platformBrowserDynamic(providers).bootstrapModule(AppBrowserModule)
  .catch(err => console.error(err));
});
