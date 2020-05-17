import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';
import { TagCategory } from '../../entities/tag-category';

@Injectable({
  providedIn: 'root'
})
export class TagCategoryService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public pageTagCategories(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<TagCategory>>(`${this.baseUrl}/web/v2/TagCategory/page`, request).toPromise();
  }

  public getTagCategories(include_relations: boolean) {
    return this.httpClient.get<TagCategory[]>(`${this.baseUrl}/web/v2/TagCategory`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getTagCategory(id: number, include_relations: boolean) {
    return this.httpClient.get<TagCategory>(`${this.baseUrl}/web/v2/TagCategory/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public createTagCategory(tagCategory: TagCategory) {
    return this.httpClient.post<TagCategory>(`${this.baseUrl}/web/v2/TagCategory`, tagCategory).toPromise();
  }

  public updateTagCategory(tagCategory: TagCategory) {
    return this.httpClient.put<TagCategory>(`${this.baseUrl}/web/v2/TagCategory/${tagCategory.id}`, tagCategory).toPromise();
  }

  public deleteTagCategory(tagCategory: TagCategory) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/TagCategory/${tagCategory.id}`).toPromise();
  }
}
