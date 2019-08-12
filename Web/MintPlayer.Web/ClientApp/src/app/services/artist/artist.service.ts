import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Artist } from '../../interfaces/artist';

@Injectable({
  providedIn: 'root'
})
export class ArtistService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getArtists(include_relations: boolean, count: number = 20, page: number = 1) {
    return this.httpClient.get<Artist[]>(`${this.baseUrl}/api/artist`, {
      headers: {
        'include_relations': String(include_relations),
        'count': String(count),
        'page': String(page)
      }
    });
  }

  public getArtist(id: number, include_relations: boolean) {
    return this.httpClient.get<Artist>(`${this.baseUrl}/api/artist/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    });
  }

  public createArtist(artist: Artist) {
    return this.httpClient.post<Artist>(`${this.baseUrl}/api/artist`, { artist });
  }

  public updateArtist(artist: Artist) {
    return this.httpClient.put(`${this.baseUrl}/api/artist/${artist.id}`, { artist });
  }

  public deleteArtist(artist: Artist) {
    return this.httpClient.delete(`${this.baseUrl}/api/artist/${artist.id}`);
  }
}
