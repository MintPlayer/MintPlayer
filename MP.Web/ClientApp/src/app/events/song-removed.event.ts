import { Song } from '../entities/song';

export class SongRemovedEvent {
  constructor(data?: Partial<SongRemovedEvent>) {
    if (data != null) {
      this.index = data.index;
      this.song = data.song;
    }
  }

  public index: number;
  public song: Song;
}
