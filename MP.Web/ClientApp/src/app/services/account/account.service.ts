import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserData } from '../../entities/user-data';
import { User } from '../../entities/user';
import { LoginResult } from '../../entities/login-result';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public register(data: UserData) {
    return this.httpClient.post(`${this.baseUrl}/web/v2/Account/register`, data).toPromise();
  }
  public login(email: string, password: string) {
    return this.httpClient.post<LoginResult>(`${this.baseUrl}/web/v2/Account/login`, { email, password }).toPromise();
  }
  public getProviders() {
    return this.httpClient.get<string[]>(`${this.baseUrl}/web/v2/account/providers`).toPromise();
  }
  public getLogins() {
    return this.httpClient.get<string[]>(`${this.baseUrl}/web/v2/account/logins`).toPromise();
  }
  public removeLogin(provider: string) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/account/logins/${provider}`).toPromise();
  }
  public currentUser() {
    return this.httpClient.get<User>(`${this.baseUrl}/web/v2/Account/current-user`).toPromise();
  }
  public currentRoles() {
    return this.httpClient.get<string[]>(`${this.baseUrl}/web/v2/Account/roles`).toPromise();
  }
  public logout() {
    return this.httpClient.post(`${this.baseUrl}/web/v2/Account/logout`, {}).toPromise();
  }
}
