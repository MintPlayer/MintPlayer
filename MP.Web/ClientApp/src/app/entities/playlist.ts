import { User } from './user';
import { Song } from './song';

export interface Playlist {
  id: number;
  user: User;
  description: string;
  tracks: Song[];
}
