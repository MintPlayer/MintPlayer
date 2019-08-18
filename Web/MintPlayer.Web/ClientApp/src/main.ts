import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers: StaticProvider[] = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'PEOPLE', useValue: null },
  { provide: 'PERSON', useValue: null },
  { provide: 'ARTISTS', useValue: null },
  { provide: 'ARTIST', useValue: null },
  { provide: 'SONGS', useValue: null },
  { provide: 'SONG', useValue: null },
  { provide: 'MEDIUMTYPES', useValue: null },
  { provide: 'MEDIUMTYPE', useValue: null },
];

if (environment.production) {
  enableProdMode();
}

document.addEventListener('DOMContentLoaded', () => {
  platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.error(err));
});
