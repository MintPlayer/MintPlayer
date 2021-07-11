import { Component, OnInit, OnDestroy, Inject, ViewChild, AfterViewInit } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { HtmlLinkHelper } from '../../helpers/html-link.helper';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  constructor(
    private metaService: Meta,
    @Inject(BASE_URL) private baseUrl: string,
    private htmlLink: HtmlLinkHelper
  ) {
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
  }

  //#region Website+Organization LD+json
  websiteLd = {
    '@context': 'http://schema.org',
    '@type': 'WebSite',
    'name': 'MintPlayer',
    'url': 'https://mintplayer.com',
    'sameAs': [
      'https://www.facebook.com/MintPlayer/',
      'https://twitter.com/MintPlayerSite',
      'https://github.com/MintPlayer'
    ],
    "potentialAction": {
      "@type": "SearchAction",
      "target": "https://mintplayer.com/search/{search_term}",
      "query-input": "required name=search_term"
    }
  };
  organizationLd = {
    '@context': 'http://schema.org',
    '@type': 'Organization',
    'name': 'MintPlayer',
    'legalName': 'MintPlayer',
    'url': 'https://mintplayer.com',
    'logo': 'https://mintplayer.com/favicon.ico',
    'foundingDate': 2019,
    'founders': [{
      '@type': 'Person',
      'name': 'Pieterjan De Clippel'
    }],
    'contactPoint': {
      '@type': 'ContactPoint',
      'contactType': 'customer support',
      'email': 'info@mintplayer.com'
    },
    'sameAs': [
      'https://www.facebook.com/MintPlayer/',
      'https://twitter.com/MintPlayerSite',
      'https://github.com/MintPlayer'
    ]
  };
  //#endregion

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
    this.basicMetaTags = this.metaService.addTags([{
      itemprop: 'name',
      content: 'MintPlayer'
    }, {
      name: 'description',
      itemprop: 'description',
      content: 'MintPlayer is an open-source project that lets you keep track of the music you like. Start building your playlist now.'
    }, {
      itemprop: 'image',
      content: `${this.baseUrl}/assets/logo/music_note_72.png`
    }, {
      itemprop: 'publisher',
      name: 'creator',
      content: 'MintPlayer LLC'
    }, {
      itemprop: 'copyrightYear',
      content: new Date().getFullYear().toString()
    }, {
      itemprop: 'copyrightHolder',
      content: 'MintPlayer LLC'
    }, {
      itemprop: 'isFamilyFriendly',
      content: 'true'
    }]);
  }
  private addOpenGraphTags() {
    this.ogMetaTags = this.metaService.addTags([{
      property: 'og:type',
      content: 'website'
    }, {
      property: 'og:url',
      content: this.baseUrl
    }, {
      property: 'og:title',
      content: 'MintPlayer - Open-source music player'
    }, {
      property: 'og:description',
      content: 'MintPlayer is an open-source project that lets you keep track of the music you like. Start building your playlist now.'
    }, {
      property: 'og:image',
      content: `${this.baseUrl}/assets/logo/music_note_512.png`
    }]);
  }
  private addTwitterCard() {
    this.twitterMetaTags = this.metaService.addTags([{
      property: 'twitter:card',
      content: 'summary'
    }, {
      property: 'twitter:url',
      content: this.baseUrl
    }, {
      property: 'twitter:image',
      content: `${this.baseUrl}/assets/logo/music_note_72.png`
    }, {
      property: 'twitter:title',
      content: 'MintPlayer - Open-source music player'
    }, {
      property: 'twitter:description',
      content: 'Welcome to MintPlayer. Start building your playlist now.'
    }]);
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
}
