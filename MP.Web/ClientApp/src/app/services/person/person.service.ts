import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
//import { BASE_URL } from '@mintplayer/ng-base-url';
import { Person } from '../../entities/person';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string, @Inject('API_VERSION') private apiVersion: string) {
  }

  public pagePeople(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Person>>(`${this.baseUrl}/web/${this.apiVersion}/person/page`, request).toPromise();
  }

  public getPeople(include_relations: boolean) {
    return this.httpClient.get<Person[]>(`${this.baseUrl}/web/${this.apiVersion}/person`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getPerson(id: number, include_relations: boolean) {
    return this.httpClient.get<Person>(`${this.baseUrl}/web/${this.apiVersion}/person/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public pageFavoritePeople(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Person>>(`${this.baseUrl}/web/${this.apiVersion}/person/favorite`, request).toPromise();
  }

  public getFavoritePeople() {
    return this.httpClient.get<Person[]>(`${this.baseUrl}/web/${this.apiVersion}/person/favorite`).toPromise();
  }

  public createPerson(person: Person) {
    return this.httpClient.post<Person>(`${this.baseUrl}/web/${this.apiVersion}/person`, person).toPromise();
  }

  public updatePerson(person: Person) {
    return this.httpClient.put<Person>(`${this.baseUrl}/web/${this.apiVersion}/person/${person.id}`, person).toPromise();
  }

  public deletePerson(person: Person) {
    return this.httpClient.delete(`${this.baseUrl}/web/${this.apiVersion}/person/${person.id}`).toPromise();
  }
}
