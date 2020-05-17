import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { HtmlLinkHelper } from '../../helpers/html-link.helper';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  constructor(private metaService: Meta, @Inject('BASE_URL') private baseUrl: string, private htmlLink: HtmlLinkHelper) {
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
  private metaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  ngOnInit() {
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
      content: 'Welcome to MintPlayer. Start building your playlist now.'
    }, {
      property: 'og:image',
      content: `${this.baseUrl}/assets/logo/music_note_512.png`
    }]);
    this.metaTags = this.metaService.addTags([{
      itemprop: 'name',
      content: 'MintPlayer'
    }, {
      name: 'description',
      itemprop: 'description',
      content: 'Welcome to MintPlayer. Start building your playlist now.'
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
    this.htmlLink.setCanonicalWithoutQuery();
  }
  ngOnDestroy() {
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.htmlLink.unset('canonical');
  }
  //#endregion
}
