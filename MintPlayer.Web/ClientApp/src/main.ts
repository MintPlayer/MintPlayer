import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// <head> is not hydrated. @mintplayer/ng-seo's [jsonLd] and [canonicalUrl] directives always append a
// fresh element instead of adopting the server-rendered one, so after hydration every JSON-LD block and
// the canonical link would exist twice. Drop the server-rendered copies; the directives re-create them
// on bootstrap. (Workaround until ng-seo adopts existing elements — see docs/spikes/S1-ssr/RESULT.md.)
document.head
  .querySelectorAll('script[type="application/ld+json"], link[rel="canonical"]')
  .forEach((el) => el.remove());

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
