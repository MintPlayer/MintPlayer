import { Component, OnInit, EventEmitter, Output, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { SongService } from '../../../services/song/song.service';
import { Song } from '../../../entities/song';
import { PropertyBindingType } from '@angular/compiler';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { Subscription } from 'rxjs';
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
  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('SONG') private songInj: Song, @Inject('BASE_URL') private baseUrl: string, private songService: SongService, private router: Router, private route: ActivatedRoute, private titleService: Title, private metaService: Meta, private htmlLink: HtmlLinkHelper, private urlGenerator: UrlGenerator) {
    if (serverSide === true) {
      this.setSong(songInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadSong(id);
    }
  }

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
  
  private metaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private setSong(song: Song) {
    this.song = song;

    //#region Remove the existing meta-tags on the page
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    //#endregion

    if (song != null) {
      //#region Title
      this.titleService.setTitle(`${song.title}: Video and lyrics`);
      //#endregion
      //#region <meta name="description">
      if (song.artists.length === 0) {
        this.metaTags = this.metaService.addTags([{
          name: 'description',
          content: `Video and lyrics for ${song.title}`
        }]);
      } else {
        this.metaTags = this.metaService.addTags([{
          name: 'description',
          content: `Video and lyrics for ${song.title} by ${song.artists[0].name}`
        }]);
      }
      //#endregion
      //#region AmpHtml
      this.htmlLink.set('amphtml', `${this.baseUrl}/amp/song/${this.song.id}`);
      //#endregion
      //#region OpenGraph tags
      this.ogMetaTags = this.metaService.addTags([{
        property: 'og:type',
        content: 'music.song'
      }, {
          property: 'og:url',
          content: this.urlGenerator.generateFullUrl(song)
      }, {
        property: 'og:title',
        content: song.title
      }, {
        property: 'og:image',
        content: `https://i.ytimg.com/vi/${song.youtubeId}/hqdefault.jpg`
      }, {
        property: 'og:updated_time',
        content: new Date(song.dateUpdate).toISOString()
      }]);

      if (song.artists.length === 0) {
        this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
          property: 'og:description',
          content: `Video and lyrics for ${song.title}`
        }]));
      } else {
        this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
          property: 'og:description',
          content: `Video and lyrics for ${song.title} by ${song.artists[0].name}`
        }]));
      }

      if (song.youtubeId !== null) {
        this.ogMetaTags = this.ogMetaTags.concat(this.metaService.addTags([{
          property: 'og:video',
          content: `https://www.youtube.com/embed/${song.youtubeId}`
        }]));
      }

      song.artists.forEach((artist) => {
        this.ogMetaTags.push(this.metaService.addTag({
          property: 'og:musician',
          content: this.urlGenerator.generateFullUrl(artist)
        }));
      });
      //#endregion
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
      //#region TwitterCard
      if (song.youtubeId) {
        this.twitterMetaTags = this.metaService.addTags([{
          property: 'twitter:card',
          content: 'player'
        }, {
          property: 'twitter:image',
          content: `http://i.ytimg.com/vi/${song.youtubeId}/hqdefault.jpg`
        }, {
          property: 'twitter:player',
          content: `https://www.youtube.com/embed/${song.youtubeId}`
        }, {
          property: 'twitter:player:width',
          content: '480'
        }, {
          property: 'twitter:player:height',
          content: '270'
        }]);
      } else {
        this.twitterMetaTags = this.metaService.addTags([{
          property: 'twitter:card',
          content: 'summary_large_image'
        }, {
          property: 'twitter:image',
          content: `${this.baseUrl}/assets/logo/music_note_72.png`
        }]);
      }
      //#endregion
    }
  }

  private routeParamsSubscription: Subscription;
  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.routeParamsSubscription = this.route.params.subscribe((routeParams) => {
    	this.loadSong(routeParams.id);
    });
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.htmlLink.unset('amphtml');
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    if (this.routeParamsSubscription !== null) {
      this.routeParamsSubscription.unsubscribe();
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

  public song: Song = {
    id: 0,
    title: '',
    released: null,
    artists: [],
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
