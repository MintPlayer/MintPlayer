import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MediumType } from '../../entities/medium-type';

@Injectable({
  providedIn: 'root'
})
export class MediumTypeService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getMediumTypes(include_relations: boolean) {
    return this.httpClient.get<MediumType[]>(`${this.baseUrl}/web/v2/mediumtype`, {
      headers: {
        'include_relations': String(include_relations),
      }
    }).toPromise();
  }

  public getMediumType(id: number, include_relations: boolean) {
    return this.httpClient.get<MediumType>(`${this.baseUrl}/web/v2/mediumtype/${id}`, {
      headers: {
        'include_relations': String(include_relations),
      }
    }).toPromise();
  }

  public createMediumType(mediumtype: MediumType) {
    return this.httpClient.post<MediumType>(`${this.baseUrl}/web/v2/mediumtype`, mediumtype).toPromise();
  }

  public updateMediumType(mediumtype: MediumType) {
    return this.httpClient.put<MediumType>(`${this.baseUrl}/web/v2/mediumtype/${mediumtype.id}`, mediumtype).toPromise();
  }

  public deleteMediumType(mediumtype: MediumType) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/mediumtype/${mediumtype.id}`).toPromise();
  }
}
