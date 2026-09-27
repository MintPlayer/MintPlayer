import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Meta } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { SeoDirective } from '@mintplayer/ng-seo/seo';
import { JsonLdDirective } from '@mintplayer/ng-seo/json-ld';
import { CanonicalUrlDirective } from '@mintplayer/ng-seo/canonical-url';
import { map, switchMap } from 'rxjs';
import { PublicSong, PublicSongService } from './public-song.service';

const SITE_URL = 'https://www.mintplayer.com';

/**
 * Public song detail page (`/song/:id`) — server-rendered via MintPlayer.AspNetCore.SpaServices
 * .Prerendering (spike S1). Renders title, artists and release date, and emits OG meta + a
 * schema.org MusicRecording JSON-LD block (@mintplayer/ng-seo).
 */
@Component({
  selector: 'app-song-page',
  imports: [DatePipe, SeoDirective, JsonLdDirective, CanonicalUrlDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (song(); as song) {
      <ng-container seo [title]="song.title + ' | MintPlayer'" [description]="description()" [commands]="['/song', song.id]"></ng-container>
      <ng-container canonicalUrl [commands]="['/song', song.id]"></ng-container>
      <ng-container [jsonLd]="jsonLd()"></ng-container>

      <article class="container py-4">
        <h1>{{ song.title }}</h1>
        @if (song.artists.length) {
          <p class="lead">
            by
            @for (artist of song.artists; track artist.id; let last = $last) {
              <span>{{ artist.name }}</span>@if (!last) {<span>, </span>}
            }
          </p>
        }
        @if (song.released) {
          <p>Released <time [attr.datetime]="song.released">{{ song.released | date: 'longDate' }}</time></p>
        }
      </article>
    } @else if (song() === null) {
      <article class="container py-4">
        <h1>Song not found</h1>
      </article>
    }
  `,
})
export class SongPage {
  private readonly songs = inject(PublicSongService);
  private readonly meta = inject(Meta);

  /** undefined = loading, null = not found. */
  protected readonly song = toSignal(
    inject(ActivatedRoute).paramMap.pipe(
      map((p) => p.get('id') ?? ''),
      switchMap((id) => this.songs.get(id)),
    ),
  );

  protected readonly description = computed(() => {
    const s = this.song();
    if (!s) return '';
    const by = s.artists.map((a) => a.name).join(', ');
    return by ? `${s.title} by ${by} on MintPlayer` : `${s.title} on MintPlayer`;
  });

  protected readonly jsonLd = computed(() => {
    const s = this.song();
    return s ? musicRecording(s) : null;
  });

  constructor() {
    // ng-seo's [seo] adds its own <meta name="description"> (Meta.addTag) next to index.html's
    // site-wide default rather than replacing it — drop the existing one(s) first.
    this.meta.getTags("name='description'").forEach((tag) => this.meta.removeTagElement(tag));
    // og:type is not covered by ng-seo's [seo] directive (it emits og:url/og:title/og:description).
    effect(() => {
      if (this.song()) this.meta.updateTag({ property: 'og:type', content: 'music.song' });
    });
  }
}

function musicRecording(s: PublicSong) {
  return {
    '@context': 'https://schema.org',
    '@type': 'MusicRecording',
    '@id': `${SITE_URL}/song/${s.id}`,
    name: s.title,
    url: `${SITE_URL}/song/${s.id}`,
    ...(s.released ? { datePublished: s.released } : {}),
    ...(s.modifiedAt ? { dateModified: s.modifiedAt } : {}),
    byArtist: s.artists.map((a) => ({
      '@type': 'MusicGroup',
      name: a.name,
      url: `${SITE_URL}/artist/${a.id}`,
    })),
  };
}
