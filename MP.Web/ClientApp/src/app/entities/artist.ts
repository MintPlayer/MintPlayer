import { Subject } from './subject';
import { Person } from './person';
import { Song } from './song';

export interface Artist extends Subject {
  name: string;
  yearStarted: number;
  yearQuit: number;

  text: string;

  currentMembers: Person[];
  pastMembers: Person[];
  songs: Song[];
}
