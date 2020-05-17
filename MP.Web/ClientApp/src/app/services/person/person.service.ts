import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Person } from '../../entities/person';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public pagePeople(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Person>>(`${this.baseUrl}/web/v2/person/page`, request).toPromise();
  }

  public getPeople(include_relations: boolean) {
    return this.httpClient.get<Person[]>(`${this.baseUrl}/web/v2/person`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getPerson(id: number, include_relations: boolean) {
    return this.httpClient.get<Person>(`${this.baseUrl}/web/v2/person/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public pageFavoritePeople(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Person>>(`${this.baseUrl}/web/v2/person/favorite`, request).toPromise();
  }

  public getFavoritePeople() {
    return this.httpClient.get<Person[]>(`${this.baseUrl}/web/v2/person/favorite`).toPromise();
  }

  public createPerson(person: Person) {
    return this.httpClient.post<Person>(`${this.baseUrl}/web/v2/person`, person).toPromise();
  }

  public updatePerson(person: Person) {
    return this.httpClient.put<Person>(`${this.baseUrl}/web/v2/person/${person.id}`, person).toPromise();
  }

  public deletePerson(person: Person) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/person/${person.id}`).toPromise();
  }
}
