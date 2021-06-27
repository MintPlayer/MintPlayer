import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { PaginationRequest, PaginationResponse } from '@mintplayer/ng-pagination';
import { Playlist } from '../../entities/playlist';
import { ePlaylistScope } from '../../enums/ePlaylistScope';

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  constructor(private httpClient: HttpClient, @Inject(BASE_URL) private baseUrl: string, @Inject('API_VERSION') private apiVersion: string) {
  }

  public pagePlaylists(request: PaginationRequest, scope: ePlaylistScope) {
    switch (scope) {
      case ePlaylistScope.My:
        return this.httpClient.post<PaginationResponse<Playlist>>(`${this.baseUrl}/web/${this.apiVersion}/playlist/my/page`, request).toPromise();
      case ePlaylistScope.Public:
        return this.httpClient.post<PaginationResponse<Playlist>>(`${this.baseUrl}/web/${this.apiVersion}/playlist/public/page`, request).toPromise();
    }
  }

  public getPlaylists(scope: ePlaylistScope, include_relations: boolean = false) {
    let headers = {
      'include_relations': String(include_relations)
    };

    switch (scope) {
      case ePlaylistScope.My:
        return this.httpClient.get<Playlist[]>(`${this.baseUrl}/web/${this.apiVersion}/playlist/my`, { headers }).toPromise();
      case ePlaylistScope.Public:
        return this.httpClient.get<Playlist[]>(`${this.baseUrl}/web/${this.apiVersion}/playlist/public`, { headers }).toPromise();
    }
  }

  public getPlaylist(id: number, include_relations: boolean = false) {
    return this.httpClient.get<Playlist>(`${this.baseUrl}/web/${this.apiVersion}/playlist/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public createPlaylist(playlist: Playlist) {
    return this.httpClient.post<Playlist>(`${this.baseUrl}/web/${this.apiVersion}/playlist`, playlist).toPromise();
  }

  public updatePlaylist(playlist: Playlist) {
    return this.httpClient.put<Playlist>(`${this.baseUrl}/web/${this.apiVersion}/playlist/${playlist.id}`, playlist).toPromise();
  }

  public deletePlaylist(playlist: Playlist) {
    return this.httpClient.delete<Playlist>(`${this.baseUrl}/web/${this.apiVersion}/playlist/${playlist.id}`).toPromise();
  }
}
