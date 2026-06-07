import { Component, inject } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { RouterOutlet } from '@angular/router';
import { JsonLdService } from './seo/json-ld.service';

/** Canonical production origin — used for structured-data URLs. */
const SITE_URL = 'https://www.mintplayer.com';
const SITE_DESCRIPTION = 'Discover artists, songs, playlists and lyrics on MintPlayer.';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly meta = inject(Meta);
  private readonly jsonLd = inject(JsonLdService);

  constructor() {
    // Default description (per-page components can override). The TitleStrategy owns <title>.
    this.meta.updateTag({ name: 'description', content: SITE_DESCRIPTION });

    // Site-wide structured data. Page-level entity schemas (MusicGroup, MusicRecording, …)
    // are layered on per page as the catalog UI lands (Phase 2+).
    this.jsonLd.setSchema('ld-website', {
      '@context': 'https://schema.org',
      '@type': 'WebSite',
      name: 'MintPlayer',
      url: SITE_URL,
    });
    this.jsonLd.setSchema('ld-organization', {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      name: 'MintPlayer',
      url: SITE_URL,
    });
  }
}
