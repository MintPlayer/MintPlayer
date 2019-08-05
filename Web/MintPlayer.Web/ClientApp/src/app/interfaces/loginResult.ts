import { User } from './account/user';

export interface LoginResult {
  status: boolean;
  platform: string;
  user: User;
  token: string;
  error: string;
  errorDescription: string;
}
