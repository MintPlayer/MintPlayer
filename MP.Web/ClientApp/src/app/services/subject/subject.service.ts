import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { Subject } from '../../entities/subject';
import { SubjectLikeResponse } from '../../entities/subject-like-response';
import { eSubjectType } from '../../enums/eSubjectType';
import { SearchResults } from '../../entities/search-results';

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  constructor(private httpClient: HttpClient, @Inject(BASE_URL) private baseUrl: string, @Inject('API_VERSION') private apiVersion: string) {
  }

  public getLikes(subject: Subject) {
    return this.httpClient.get<SubjectLikeResponse>(`${this.baseUrl}/web/${this.apiVersion}/subject/${subject.id}/likes`).toPromise();
  }

  public like(subject: Subject, like: boolean) {
    return this.httpClient.post<SubjectLikeResponse>(`${this.baseUrl}/web/${this.apiVersion}/subject/${subject.id}/likes`, like, {
      headers: {
        'Content-Type': 'application/json'
      }
    }).toPromise();
  }

  public getLikedSubjects() {
    return this.httpClient.get<Subject[]>(`${this.baseUrl}/web/${this.apiVersion}/subject/favorite`).toPromise();
  }

  public suggest(search: string, subjects: eSubjectType[]) {
    var subjects_concat = subjects.join('-');
    return this.httpClient.get<Subject[]>(`${this.baseUrl}/web/${this.apiVersion}/subject/search/suggest/${subjects_concat}/${search}`).toPromise();
  }

  public search(search: string, subjects: eSubjectType[]) {
    var subjects_concat = subjects.join('-');
    return this.httpClient.get<SearchResults>(`${this.baseUrl}/web/${this.apiVersion}/subject/search/${subjects_concat}/${search}`).toPromise();
  }
}
