import { ePlaylistPlaybutton } from '../enums/ePlaylistPlayButton';
import { Song } from '../entities/song';

export class PlayButtonClickedEvent {
  constructor(data?: Partial<PlayButtonClickedEvent>) {
    if (data != null) {
      Object.assign(this, data);
    }
  }

  public songs: Song[];
  public button: ePlaylistPlaybutton;
}
