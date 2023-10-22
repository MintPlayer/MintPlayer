import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { StaticProvider } from '@angular/core';
import { AppBrowserModule } from './app/app.browser.module';

const providers: StaticProvider[] = [];

platformBrowserDynamic(providers).bootstrapModule(AppBrowserModule)
  .catch(err => console.error(err));
