import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';
import { Playlist } from '../../entities/playlist';

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public pagePlaylists(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Playlist>>(`${this.baseUrl}/web/v2/playlist/page`, request).toPromise();
  }

  public getPlaylists(include_relations: boolean = false) {
    return this.httpClient.get<Playlist[]>(`${this.baseUrl}/web/v2/playlist`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getPlaylist(id: number, include_relations: boolean = false) {
    return this.httpClient.get<Playlist>(`${this.baseUrl}/web/v2/playlist/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public createPlaylist(playlist: Playlist) {
    return this.httpClient.post<Playlist>(`${this.baseUrl}/web/v2/playlist`, playlist).toPromise();
  }

  public updatePlaylist(playlist: Playlist) {
    return this.httpClient.put<Playlist>(`${this.baseUrl}/web/v2/playlist/${playlist.id}`, playlist).toPromise();
  }

  public deletePlaylist(playlist: Playlist) {
    return this.httpClient.delete<Playlist>(`${this.baseUrl}/web/v2/playlist/${playlist.id}`).toPromise();
  }
}
