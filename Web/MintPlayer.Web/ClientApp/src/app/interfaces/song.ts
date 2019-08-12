import { Artist } from './artist';
import { Subject } from './subject';

export interface Song extends Subject {
	title: string;
	released: Date;
	lyrics: string;

	text: string;

  artists: Artist[];
}
