import { SongWithMedium } from "../interfaces/song-with-medium";
import { VideoUrl } from "../interfaces/video-url";

export class SongRemovedEvent {
  constructor(data?: Partial<SongRemovedEvent>) {
    if (data != null) {
      this.index = data.index;
      this.song = data.song;
    }
  }

  public index: number;
  public song: SongWithMedium | VideoUrl;
}
