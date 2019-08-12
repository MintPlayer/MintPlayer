import { Person } from './person';
import { Artist } from './artist';
import { Song } from './song';

export interface SearchResults {
  people: Person[];
  artists: Artist[];
  songs: Song[];
}
