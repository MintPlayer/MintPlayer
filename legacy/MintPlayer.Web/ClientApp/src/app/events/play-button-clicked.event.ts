import { Song } from '@mintplayer/ng-client';
import { ePlaylistPlaybutton } from '../enums/ePlaylistPlayButton';

export class PlayButtonClickedEvent {
  constructor(data?: Partial<PlayButtonClickedEvent>) {
    if (data != null) {
      Object.assign(this, data);
    }
  }

  public songs: Song[];
  public button: ePlaylistPlaybutton;
}
