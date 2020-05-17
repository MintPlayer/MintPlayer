import { User } from './user';

export interface LoginResult {
  status: boolean;
  medium: string;
  platform: string;
  user: User;
	error: string;
	errorDescription: string;
}
