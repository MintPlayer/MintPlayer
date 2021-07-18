import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { API_VERSION, User } from '@mintplayer/ng-client';

@Component({
  selector: 'app-two-factor',
  templateUrl: './two-factor.component.html',
  styleUrls: ['./two-factor.component.scss']
})
export class TwoFactorComponent implements OnInit {

  constructor(private httpClient: HttpClient, private router: Router, @Inject(BASE_URL) private baseUrl: string, @Inject(API_VERSION) private apiVersion: string) {
  }

  ngOnInit() {
    //this.httpClient.get<User>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-user`).subscribe((response) => {
    //  console.log(response);
    //});
  }

  setupCode: string = '';

  verifyCode() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-login`, { code: this.setupCode, remember: false }).subscribe((backupCodes) => {
      console.log(backupCodes);
      this.router.navigate(["/"]);
    });
    return false;
  }

}
