import { Guid } from 'guid-typescript';

export interface User {
  id: Guid;
  userName: string;
  email: string;
  pictureUrl: string;
}
