import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { API_VERSION, Artist, Person, SearchResults, Song, SubjectService, SubjectType } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../helpers/html-link.helper';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { UrlGenerator } from '../../helpers/url-generator.helper';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
  providers: [
    SlugifyPipe
  ]
})
export class SearchComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(API_VERSION) apiVersion: string,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private subjectService: SubjectService,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta,
    private urlGenerator: UrlGenerator
  ) {
    this.apiVersion = apiVersion;
    this.route.paramMap.subscribe((params) => {
      if (params.has('searchTerm')) {
        this.searchterm = params.get('searchTerm');
        this.performSearch();
      }
    });
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
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
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: 'Search for music in the MintPlayer database.'
    }]);
  }
  private addOpenGraphTags() {
  }
  private addTwitterCard() {
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

  apiVersion: string = '';
  searchterm: string = '';
  searchResults: SearchResults = {
    artists: [],
    people: [],
    songs: []
  }

  doSearch() {
    this.router.navigate(['/search', this.searchterm]);
  }

  private performSearch() {
    this.subjectService.search(this.searchterm, [SubjectType.person, SubjectType.artist, SubjectType.song]).then((results) => {
      this.searchResults = results;
    }).catch((error) => {
      console.error('Could not perform search query', error);
    });
  }

  gotoSubject(subject: Person | Artist | Song) {
    this.router.navigate(this.urlGenerator.generateCommands(subject));
  }

}
