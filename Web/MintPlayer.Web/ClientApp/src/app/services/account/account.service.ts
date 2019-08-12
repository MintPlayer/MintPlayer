import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { User } from '../../interfaces/account/user';
import { LoginResult } from '../../interfaces/loginResult';
import { UserData } from '../../interfaces/account/userData';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public register(data: UserData) {
    return this.httpClient.post(`${this.baseUrl}/api/account/register`, data);
  }
  public login(email: string, password: string) {
    return this.httpClient.post<LoginResult>(
      `${this.baseUrl}/api/account/login`,
      { email, password },
      {
        headers: new HttpHeaders({
          'Use-Cookies': 'true'
        })
      });
  }
  public currentUser() {
    return this.httpClient.get<User>(`${this.baseUrl}/api/account/current-user`);
  }
  public logout() {
    return this.httpClient.post(`${this.baseUrl}/api/account/logout`, {});
  }
  public getToken() {
    return localStorage.getItem('auth_token');
  }
}
