import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';
import { Tag } from '../../entities/tag';
import { TagCategory } from '../../entities/tag-category';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public pageTags(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Tag>>(`${this.baseUrl}/web/v2/Tag/page`, request).toPromise();
  }

  public suggestTags(search: string) {
    return this.httpClient.post<Tag[]>(`${this.baseUrl}/web/v2/Tag/suggest`, { searchTerm: search }).toPromise();
  }

  public searchTags(search: string) {
    return this.httpClient.post<Tag[]>(`${this.baseUrl}/web/v2/Tag/search`, { searchTerm: search }).toPromise();
  }

  public getTags(include_relations: boolean) {
    return this.httpClient.get<Tag[]>(`${this.baseUrl}/web/v2/Tag`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getTag(id: number, include_relations: boolean) {
    return this.httpClient.get<Tag>(`${this.baseUrl}/web/v2/Tag/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public createTag(tag: Tag) {
    let clone = this.removeSubjects(tag);
    return this.httpClient.post<Tag>(`${this.baseUrl}/web/v2/Tag`, clone).toPromise();
  }

  public updateTag(tag: Tag) {
    let clone = this.removeSubjects(tag);
    return this.httpClient.put<Tag>(`${this.baseUrl}/web/v2/Tag/${tag.id}`, clone).toPromise();
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
    return this.httpClient.delete(`${this.baseUrl}/web/v2/Tag/${tag.id}`).toPromise();
  }
}
