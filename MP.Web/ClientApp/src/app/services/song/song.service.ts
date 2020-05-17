import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Song } from '../../entities/song';
import { PaginationRequest } from '../../helpers/pagination-request';
import { PaginationResponse } from '../../helpers/pagination-response';
import { Lyrics } from '../../entities/lyrics';

@Injectable({
  providedIn: 'root'
})
export class SongService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public pageSongs(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Song>>(`${this.baseUrl}/web/v2/song/page`, request).toPromise();
  }

  public getSongs(include_relations: boolean) {
    return this.httpClient.get<Song[]>(`${this.baseUrl}/web/v2/song`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public getSong(id: number, include_relations: boolean) {
    return this.httpClient.get<Song>(`${this.baseUrl}/web/v2/song/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    }).toPromise();
  }

  public pageFavoriteSongs(request: PaginationRequest) {
    return this.httpClient.post<PaginationResponse<Song>>(`${this.baseUrl}/web/v2/song/favorite`, request).toPromise();
  }

  public getFavoriteSongs() {
    return this.httpClient.get<Song[]>(`${this.baseUrl}/web/v2/song/favorite`).toPromise();
  }

  public getLyrics(songId: number) {
    return this.httpClient.get<Lyrics>(`${this.baseUrl}/web/v2/song/${songId}/lyrics`).toPromise();
  }

  public createSong(song: Song) {
    return this.httpClient.post<Song>(`${this.baseUrl}/web/v2/song`, song).toPromise();
  }

  public updateSong(song: Song) {
    return this.httpClient.put<Song>(`${this.baseUrl}/web/v2/song/${song.id}`, song).toPromise();
  }

  public updateTimeline(song: Song) {
    return this.httpClient.put(`${this.baseUrl}/web/v2/song/${song.id}/timeline`, song).toPromise();
  }

  public deleteSong(song: Song) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/song/${song.id}`).toPromise();
  }
}
