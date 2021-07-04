import { Song } from "@mintplayer/ng-client";

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
