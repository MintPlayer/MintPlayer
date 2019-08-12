import { Medium } from './medium';

export interface Subject {
  id: number;
  text: string;

  media: Medium[];
}
