import { User } from '../../entities/account/user';

export interface UserData {
  user: User;
  password: string;
  passwordConfirmation: string;
}
