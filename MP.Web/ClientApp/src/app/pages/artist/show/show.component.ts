import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ArtistService } from '../../../services/artist/artist.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { Artist } from '../../../entities/artist';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { UrlGenerator } from '../../../helpers/url-generator.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('ARTIST') private artistInj: Artist, @Inject('BASE_URL') private baseUrl: string, private artistService: ArtistService, private router: Router, private route: ActivatedRoute, private titleService: Title, private metaService: Meta, private htmlLink: HtmlLinkHelper, private urlGenerator: UrlGenerator) {
    if (serverSide === true) {
      this.setArtist(artistInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadArtist(id);
    }
  }

  artistLd: {
    '@context': 'http://schema.org',
    '@type': 'MusicGroup',
    'url': string,
    'name': string,
    'description': string,
    'track': {
      '@context': 'http://schema.org',
      '@type': 'MusicRecording',
      'url': string,
      'name': string,
      'image': string,
      'datePublished': Date
    }[]
  } = {
    '@context': 'http://schema.org',
    '@type': 'MusicGroup',
    'url': '',
    'name': '',
    'description': '',
    'track': []
  };

  private loadArtist(id: number) {
    this.artistService.getArtist(id, true).then((artist) => {
      this.setArtist(artist);
    }).catch((error) => {
      console.error('Could not get artist', error);
    });
  }

  private metaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private setArtist(artist: Artist) {
    this.artist = artist;

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

    if (artist != null) {
      //#region Title
      this.titleService.setTitle(artist.name);
      //#endregion
      //#region <meta name="description">
      this.metaTags = this.metaService.addTags([{
        name: 'description',
        content: `Songs, music videos and lyrics for ${artist.name}`
      }]);
      //#endregion
      //#region OpenGraph tags
      this.ogMetaTags = this.metaService.addTags([{
        property: 'og:type',
        content: 'profile'
      }, {
        property: 'og:url',
          content: this.urlGenerator.generateFullUrl(artist)
      }, {
        property: 'og:title',
        content: artist.name
      }, {
        property: 'og:description',
        content: `Songs, music videos and lyrics for ${artist.name}`
      }, {
        property: 'og:updated_time',
        content: new Date(artist.dateUpdate).toISOString()
      }]);
      //#endregion
      //#region LD+json
      this.artistLd = {
        '@context': 'http://schema.org',
        '@type': 'MusicGroup',
        'url': this.urlGenerator.generateFullUrl(artist),
        'name': artist.name,
        'description': artist.name,
        'track': artist.songs.map((song) => {
          return {
            '@context': 'http://schema.org',
            '@type': 'MusicRecording',
            'url': this.urlGenerator.generateFullUrl(song),
            'name': song.title,
            'image': song.youtubeId ? `http://i.ytimg.com/vi/${song.youtubeId}/hqdefault.jpg` : '',
            'datePublished': new Date(song.released)
          };
        })
      };
      //#endregion
      //#region TwitterCard
      this.twitterMetaTags = this.metaService.addTags([{
        property: 'twitter:card',
        content: 'summary'
      }, {
        property: 'twitter:url',
        content: this.urlGenerator.generateFullUrl(artist)
      }, {
        property: 'twitter:image',
        content: `${this.baseUrl}/assets/logo/music_note_72.png`
      }, {
        property: 'twitter:title',
        content: artist.name
      }, {
        property: 'twitter:description',
        content: `Songs, music videos and lyrics for ${artist.name}`
      }]);
      //#endregion
    }
  }

  public deleteArtist() {
    this.artistService.deleteArtist(this.artist).then(() => {
      this.router.navigate(['artist']);
    }).catch((error) => {
      console.error('Could not delete artist', error);
    });
  }

  public artist: Artist = {
    id: 0,
    name: '',
    yearStarted: null,
    yearQuit: null,
    currentMembers: [],
    pastMembers: [],
    media: [],
    songs: [],
    tags: [],
    text: '',
    dateUpdate: null
  };

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
  }
}
