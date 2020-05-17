import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { SubjectService } from '../../services/subject/subject.service';
import { SearchResults } from '../../entities/search-results';
import { eSubjectType } from '../../enums/eSubjectType';
import { Person } from '../../entities/person';
import { Artist } from '../../entities/artist';
import { Song } from '../../entities/song';
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

  constructor(private router: Router, private route: ActivatedRoute, private subjectService: SubjectService, private htmlLink: HtmlLinkHelper, private urlGenerator: UrlGenerator) {
    this.route.paramMap.subscribe((params) => {
      if (params.has('searchTerm')) {
        this.searchterm = params.get('searchTerm');
        this.performSearch();
      }
    });
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

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
    this.subjectService.search(this.searchterm, [eSubjectType.person, eSubjectType.artist, eSubjectType.song]).then((results) => {
      this.searchResults = results;
    }).catch((error) => {
      console.error('Could not perform search query', error);
    });
  }

  gotoSubject(subject: Person | Artist | Song) {
    this.router.navigate(this.urlGenerator.generateCommands(subject));
  }

}
