import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from '../../interfaces/subject';
import { SubjectLikeResponse } from '../../interfaces/subjectLikeResponse';
import { eSubjectType } from '../../enums/eSubjectType';
import { SearchResults } from '../../interfaces/SearchResults';

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getLikes(subject: Subject) {
    return this.httpClient.get<SubjectLikeResponse>(`${this.baseUrl}/api/subject/${subject.id}/likes`);
  }

  public like(subject: Subject, like: boolean) {
    return this.httpClient.post<SubjectLikeResponse>(`${this.baseUrl}/api/subject/${subject.id}/likes`, like, {
      headers: {
        "Content-Type": "application/json"
      }
    });
  }

  public suggest(search: string, subjects: eSubjectType[]) {
    var subjects_concat = subjects.join('-');
    return this.httpClient.get<Subject[]>(`${this.baseUrl}/api/subject/search/suggest/${subjects_concat}/${search}`);
  }

  public search(search: string, subjects: eSubjectType[]) {
    var subjects_concat = subjects.join('-');
    return this.httpClient.get<SearchResults>(`${this.baseUrl}/api/subject/search/${subjects_concat}/${search}`);
  }
}
