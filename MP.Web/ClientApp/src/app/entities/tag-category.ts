import { Tag } from './tag';

export interface TagCategory {
  id: number;
  color: string;
  description: string;
  tags: Tag[];
}
