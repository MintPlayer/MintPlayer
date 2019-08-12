import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Song } from '../../interfaces/song';

@Injectable({
  providedIn: 'root'
})
export class SongService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getSongs(include_relations: boolean, count: number = 20, page: number = 1) {
    return this.httpClient.get<Song[]>(`${this.baseUrl}/api/song`, {
      headers: {
        'include_relations': String(include_relations),
        'count': String(count),
        'page': String(page)
      }
    });
  }

  public getSong(id: number, include_relations: boolean) {
    return this.httpClient.get<Song>(`${this.baseUrl}/api/song/${id}`, {
      headers: {
        'include_relations': String(include_relations)
      }
    });
  }

  public createSong(song: Song) {
    return this.httpClient.post<Song>(`${this.baseUrl}/api/song`, { song });
  }

  public updateSong(song: Song) {
    return this.httpClient.put(`${this.baseUrl}/api/song/${song.id}`, { song });
  }

  public deleteSong(song: Song) {
    return this.httpClient.delete(`${this.baseUrl}/api/song/${song.id}`);
  }
}
