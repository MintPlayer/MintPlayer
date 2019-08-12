import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MediumType } from '../../interfaces/medium-type';

@Injectable({
  providedIn: 'root'
})
export class MediumTypeService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getMediumTypes(include_relations: boolean) {
    return this.httpClient.get<MediumType[]>(`${this.baseUrl}/api/mediumtype`, {
      headers: {
        'include_relations': String(include_relations),
      }
    });
  }

  public getMediumType(id: number, include_relations: boolean) {
    return this.httpClient.get<MediumType>(`${this.baseUrl}/api/mediumtype/${id}`, {
      headers: {
        'include_relations': String(include_relations),
      }
    });
  }

  public createMediumType(mediumtype: MediumType) {
    return this.httpClient.post<MediumType>(`${this.baseUrl}/api/mediumtype`, { mediumtype });
  }

  public updateMediumType(mediumtype: MediumType) {
    return this.httpClient.put<MediumType>(`${this.baseUrl}/api/mediumtype/${mediumtype.id}`, { mediumtype });
  }

  public deleteMediumType(mediumtype: MediumType) {
    return this.httpClient.delete(`${this.baseUrl}/api/mediumtype/${mediumtype.id}`);
  }
}
