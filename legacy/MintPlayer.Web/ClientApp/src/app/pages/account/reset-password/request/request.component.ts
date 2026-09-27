import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { MINTPLAYER_API_VERSION } from '@mintplayer/ng-client';

@Component({
  selector: 'app-request',
  templateUrl: './request.component.html',
  styleUrls: ['./request.component.scss']
})
export class RequestComponent implements OnInit {

  constructor(
    private httpClient: HttpClient,
    private titleService: Title,
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
  ) {
    this.titleService.setTitle('Reset your password');
  }

  baseUrl = this.baseUrlService.getBaseUrl();
  ngOnInit() {
  }

  email: string;
  emailSent: boolean;

  sendPasswordResetEmail() {
    this.httpClient.post(`${this.baseUrl}/web/${this.apiVersion}/Account/password/reset`, { email: this.email })
      .subscribe({
        next: (response) => {
          this.emailSent = true;
        }, error: (error) => {
          console.log('Could not send password reset email', error);
        }
      });
  }
}
