import { User } from './user';
import { Song } from './song';
import { ePlaylistAccessibility } from '../enums/ePlaylistAccessibility';

export interface Playlist {
  id: number;
  user: User;
  description: string;
  accessibility: ePlaylistAccessibility;
  tracks: Song[];
}
