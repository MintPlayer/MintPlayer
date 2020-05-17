import { Subject } from './subject';
import { Artist } from './artist';

export interface Person extends Subject {
  firstName: string;
  lastName: string;
  born: Date;
  died: Date;

  text: string;

  artists: Artist[];
}
