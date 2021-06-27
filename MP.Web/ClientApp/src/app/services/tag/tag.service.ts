import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { PaginationRequest, PaginationResponse } from '@mintplayer/ng-pagination';
import { Tag } from '../../entities/tag';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  constructor(private httpClient: HttpClient, @Inject(BASE_URL) private baseUrl: string, @Inject('API_VERSION') private apiVersion: string) {
  }

  public pageTags(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Tag>>(`${this.baseUrl}/web/${this.apiVersion}/Tag/page`, request).toPromise();
  }

  public suggestTags(search: string) {
    return this.httpClient.post<Tag[]>(`${this.baseUrl}/web/${this.apiVersion}/Tag/suggest`, { searchTerm: search }).toPromise();
  }

  public searchTags(search: string) {
    return this.httpClient.post<Tag[]>(`${this.baseUrl}/web/${this.apiVersion}/Tag/search`, { searchTerm: search }).toPromise();
  }

  public getTags(include_relations: boolean) {
    return this.httpClient.get<Tag[]>(`${this.baseUrl}/web/${this.apiVersion}/Tag`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getTag(id: number, include_relations: boolean) {
    return this.httpClient.get<Tag>(`${this.baseUrl}/web/${this.apiVersion}/Tag/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public createTag(tag: Tag) {
    let clone = this.removeSubjects(tag);
    return this.httpClient.post<Tag>(`${this.baseUrl}/web/${this.apiVersion}/Tag`, clone).toPromise();
  }

  public updateTag(tag: Tag) {
    let clone = this.removeSubjects(tag);
    return this.httpClient.put<Tag>(`${this.baseUrl}/web/${this.apiVersion}/Tag/${tag.id}`, clone).toPromise();
  }

  private removeSubjects(tag: Tag) {
    // Remove "subjects" from tag
    let clone = Object.assign({}, tag);
    clone.subjects = [];
    if (clone.parent !== null) {
      clone.parent.subjects = [];
    }

    return clone;
  }

  public deleteTag(tag: Tag) {
    return this.httpClient.delete(`${this.baseUrl}/web/${this.apiVersion}/Tag/${tag.id}`).toPromise();
  }
}
