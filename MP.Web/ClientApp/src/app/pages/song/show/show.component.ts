import { Component, OnInit, EventEmitter, Output, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { Song, SongService } from '@mintplayer/ng-client';
import { Subscription } from 'rxjs';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { UrlGenerator } from '../../../helpers/url-generator.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss'],
  providers: [
    SlugifyPipe
  ]
})
export class ShowComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('SONG') private songInj: Song,
    @Inject(BASE_URL) private baseUrl: string,
    private songService: SongService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper,
    private urlGenerator: UrlGenerator
  ) {
    if (serverSide === true) {
      this.setSong(songInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadSong(id);
    }
  }

  private routeParamsSubscription: Subscription;

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.routeParamsSubscription = this.route.params.subscribe((routeParams) => {
      this.loadSong(routeParams.id);
    });

    let wrapLyrics = localStorage.getItem('wrapLyrics');
    if (wrapLyrics === null) {
      this.wrapLyrics = true;
    } else {
      this.wrapLyrics = wrapLyrics !== 'false';
    }
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.htmlLink.unset('amphtml');
    this.removeMetaTags();
    if (this.routeParamsSubscription !== null) {
      this.routeParamsSubscription.unsubscribe();
    }
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    if (this.song.artists.length === 0) {
      this.basicMetaTags = this.metaService.addTags([{
        name: 'description',
        content: `Video and lyrics for ${this.song.title}`
      }]);
    } else {
      this.basicMetaTags = this.metaService.addTags([{
        name: 'description',
        content: `Video and lyrics for ${this.song.title} by ${this.song.artists[0].name}`
      }]);
    }
  }
  private addOpenGraphTags() {
    this.ogMetaTags = this.metaService.addTags([{
      property: 'og:type',
      content: 'music.song'
    }, {
      property: 'og:url',
      content: this.urlGenerator.generateFullUrl(this.song)
    }, {
      property: 'og:title',
      content: this.song.title
    }, {
      property: 'og:image',
      content: `https://i.ytimg.com/vi/${this.song.youtubeId}/hqdefault.jpg`
    }, {
      property: 'og:updated_time',
      content: new Date(this.song.dateUpdate).toISOString()
    }]);

    if (this.song.artists.length === 0) {
      this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
        property: 'og:description',
        content: `Video and lyrics for ${this.song.title}`
      }]));
    } else {
      this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
        property: 'og:description',
        content: `Video and lyrics for ${this.song.title} by ${this.song.artists[0].name}`
      }]));
    }

    if (this.song.youtubeId !== null) {
      this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
        property: 'og:video',
        content: `https://www.youtube.com/embed/${this.song.youtubeId}`
      }]));
    }

    this.song.artists.forEach((artist) => {
      this.ogMetaTags.push(this.metaService.addTag({
        property: 'og:musician',
        content: this.urlGenerator.generateFullUrl(artist)
      }));
    });
  }
  private addTwitterCard() {
    let getSongDescription = (song: Song) => {
      if (song.artists.length === 0) {
        return `Video and lyrics for ${song.title}`;
      } else {
        return `Video and lyrics for ${song.title} by ${song.artists[0].name}`;
      }
    };

    if (this.song.youtubeId) {
      this.twitterMetaTags = this.metaService.addTags([{
        property: 'twitter:card',
        content: 'player'
      }, {
        property: 'twitter:image',
        content: `http://i.ytimg.com/vi/${this.song.youtubeId}/hqdefault.jpg`
      }, {
        property: 'twitter:player',
        content: `https://www.youtube.com/embed/${this.song.youtubeId}`
      }, {
        property: 'twitter:player:width',
        content: '480'
      }, {
        property: 'twitter:player:height',
        content: '270'
      }, {
        property: 'twitter:title',
        content: this.song.title
      }, {
        property: 'twitter:description',
        content: getSongDescription(this.song)
      }]);
    } else {
      this.twitterMetaTags = this.metaService.addTags([{
        property: 'twitter:card',
        content: 'summary_large_image'
      }, {
        property: 'twitter:image',
        content: `${this.baseUrl}/assets/logo/music_note_72.png`
      }, {
        property: 'twitter:title',
        content: this.song.title
      }, {
        property: 'twitter:description',
        content: getSongDescription(this.song)
      }]);
    }
  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion

  songLd = {
    '@context': 'http://schema.org',
    '@type': 'MusicRecording',
    'url': '',
    'name': '',
    'image': '',
    'datePublished': null,
    'byArtist': []
  };
  videoLd: {
    '@context': 'http://schema.org',
    '@type': 'VideoObject',
    'name': string,
    'description': string,
    'thumbnailUrl': string,
    'contentUrl': string,
    'embedUrl': string,
    'uploadDate': Date
  } = null;

  private loadSong(id: number) {
    this.songService.getSong(id, true).then((song) => {
      this.setSong(song);
    }).catch((error) => {
      console.error('Could not fetch song', error);
    });
  }

  private setSong(song: Song) {
    this.song = song;
    this.removeMetaTags();

    if (song != null) {
      this.titleService.setTitle(`${song.title}: Video and lyrics`);
      this.addMetaTags();
      this.htmlLink.set('amphtml', `${this.baseUrl}/amp/song/${this.song.id}`);
      //#region LD+json
      this.songLd = {
        '@context': 'http://schema.org',
        '@type': 'MusicRecording',
        'url': this.urlGenerator.generateFullUrl(song),
        'name': song.title,
        'image': song.youtubeId ? `http://i.ytimg.com/vi/${song.youtubeId}/hqdefault.jpg` : '',
        'datePublished': new Date(song.released).toISOString(),
        'byArtist': song.artists.map((artist) => {
          return {
            '@context': 'http://schema.org',
            '@type': 'MusicGroup',
            'name': artist.name,
            'url': this.urlGenerator.generateFullUrl(artist)
          };
        })
      };
      if (song.youtubeId) {
        this.videoLd = {
          '@context': 'http://schema.org',
          '@type': 'VideoObject',
          'name': song.title,
          'description': song.description,
          'thumbnailUrl': `http://i.ytimg.com/vi/${song.youtubeId}/hqdefault.jpg`,
          'contentUrl': `https://www.youtube.com/watch?v=${song.youtubeId}`,
          'embedUrl': `https://www.youtube.com/embed/${song.youtubeId}`,
          'uploadDate': song.dateUpdate
        }
      } else {
        this.videoLd = null;
      }
      //#endregion
    }
  }

  public deleteSong() {
    this.songService.deleteSong(this.song).then(() => {
      this.router.navigate(['song']);
    }).catch((error) => {
      console.error('Could not delete song', error);
    });
  }

  @Output() addToPlaylist: EventEmitter<Song> = new EventEmitter();
  public doAddToPlaylist() {
    this.addToPlaylist.emit(this.song);
  }

  toggleWrapLyrics() {
    this.wrapLyrics = !this.wrapLyrics;
    localStorage.setItem('wrapLyrics', String(this.wrapLyrics));
  }

  wrapLyrics: boolean;
  song: Song = {
    id: 0,
    title: '',
    released: null,
    artists: [],
    uncreditedArtists: [],
    media: [],
    tags: [],
    lyrics: {
      text: '',
      timeline: []
    },
    text: '',
    youtubeId: '',
    dailymotionId: '',
    playerInfo: null,
    description: '',
    dateUpdate: null
  };
}
