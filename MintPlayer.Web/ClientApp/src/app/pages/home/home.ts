import { Component } from '@angular/core';
import { CanonicalUrlDirective } from '@mintplayer/ng-seo/canonical-url';

@Component({
  selector: 'app-home',
  imports: [CanonicalUrlDirective],
  template: `
    <!-- Per-page canonical URL via @mintplayer/ng-seo. The document title is handled
         site-wide by MintPlayerTitleStrategy ("Home | MintPlayer"); the default meta
         description comes from index.html. Per-page <ng-container seo …> (title +
         description + og: tags) gets added on content pages in Phase 2+. -->
    <ng-container canonicalUrl [commands]="['/home']"></ng-container>

    <h1>MintPlayer</h1>
    <p class="text-body-secondary">
      Running on MintPlayer.Spark. Catalog, playlists, lyrics and the rest of the site
      are migrated incrementally from here.
    </p>
  `,
})
export class Home {}
