import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { SubjectService } from '../../services/subject/subject.service';
import { SearchResults } from '../../interfaces/SearchResults';
import { eSubjectType } from '../../enums/eSubjectType';
import { Person } from '../../interfaces/person';
import { Artist } from '../../interfaces/artist';
import { Song } from '../../interfaces/song';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {

  constructor(private router: Router, private route: ActivatedRoute, private subjectService: SubjectService) {
    this.route.paramMap.subscribe((params) => {
      if (params.has('subjects') && params.has('searchterm')) {
        this.searchterm = params.get('searchterm');
        this.searchSubjects();
      } else {
        this.searchResults = {
          artists: [],
          people: [],
          songs: []
        };
      }
    });
  }

  ngOnInit() {
  }

  searchterm: string = '';
  searchResults: SearchResults = {
    artists: [],
    people: [],
    songs: []
  }

  doSearch() {
    this.router.navigate(['/search', 'all', this.searchterm]);
  }

  searchSubjects() {
    return this.subjectService.search(this.searchterm, [eSubjectType.person, eSubjectType.artist, eSubjectType.song]).subscribe((results) => {
      this.searchResults = results;
    });
  }

  gotoSubject(subject: Person | Artist | Song) {
    if ('firstName' in subject) {
      this.router.navigate(['/person', subject.id]);
    } else if ('name' in subject) {
      this.router.navigate(['/artist', subject.id]);
    } else if ('title' in subject) {
      this.router.navigate(['/song', subject.id]);
    }
  }

}
