import { User } from './user';

export interface UserData {
  user: User;
  password: string;
  passwordConfirmation: string;
}
