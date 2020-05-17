import { Medium } from './medium';
import { Tag } from './tag';

export interface Subject {
  id: number;
  text: string;
  dateUpdate: Date;

  media: Medium[];
  tags: Tag[];
}
