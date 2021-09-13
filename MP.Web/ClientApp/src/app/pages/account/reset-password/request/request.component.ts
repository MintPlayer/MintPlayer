import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { API_VERSION } from '@mintplayer/ng-client';

@Component({
  selector: 'app-request',
  templateUrl: './request.component.html',
  styleUrls: ['./request.component.scss']
})
export class RequestComponent implements OnInit {

  constructor(
    private httpClient: HttpClient,
    private titleService: Title,
    @Inject(BASE_URL) private baseUrl: string,
    @Inject(API_VERSION) private apiVersion: string,
  ) {
    this.titleService.setTitle('Reset your password');
  }

  ngOnInit() {
  }

  email: string;
  emailSent: boolean;

  sendPasswordResetEmail() {
    this.httpClient.post(`${this.baseUrl}/web/${this.apiVersion}/Account/password/reset`, { email: this.email }).toPromise()
      .then((response) => {
        this.emailSent = true;
      }).catch((error) => {
        console.log('Could not send password reset email', error);
      });
  }
}
