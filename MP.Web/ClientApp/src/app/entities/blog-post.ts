import { User } from './user';

export interface BlogPost {
  id: number;
  title: string;
  headline: string;
  body: string;
  author: User;
  published: Date;
}
