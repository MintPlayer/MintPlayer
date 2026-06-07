import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { JsonLdDirective } from '@mintplayer/ng-seo/json-ld';

/** Canonical production origin — used for structured-data URLs. */
const SITE_URL = 'https://www.mintplayer.com';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, JsonLdDirective],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  // Site-wide structured data, rendered as <script type="application/ld+json"> by
  // @mintplayer/ng-seo's [jsonLd] directive in app.html. Page-level entity schemas
  // (MusicGroup, MusicRecording, …) layer on per page as the catalog UI lands (Phase 2+).
  protected readonly websiteSchema = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: 'MintPlayer',
    url: SITE_URL,
  };

  protected readonly organizationSchema = {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: 'MintPlayer',
    url: SITE_URL,
  };
}
