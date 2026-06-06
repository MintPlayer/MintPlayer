import { HttpClient } from '@angular/common/http';
import { Inject } from '@angular/core';
import { AfterViewInit } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { MINTPLAYER_API_VERSION } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';

@Component({
  selector: 'app-perform',
  templateUrl: './perform.component.html',
  styleUrls: ['./perform.component.scss']
})
export class PerformComponent implements OnInit, AfterViewInit {

  constructor(
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private httpClient: HttpClient,
    private titleService: Title,
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
  ) {
    this.titleService.setTitle('Enter a new password');
  }

  baseUrl = this.baseUrlService.getBaseUrl();
  ngOnInit() {
    this.email = this.route.snapshot.queryParamMap.get('email');
    this.token = this.route.snapshot.queryParamMap.get('token');
  }

  ngAfterViewInit() {
  }

  performPasswordReset() {
    this.httpClient.put(`${this.baseUrl}/web/${this.apiVersion}/Account/password/reset`, {
      email: this.email,
      token: this.token,
      newPassword: this.password,
      newPasswordConfirmation: this.passwordConfirmation
    })
      .subscribe({
        next: (response) => {
          this.success = true;
          setTimeout(() => {
            this.router.navigate(['/account', 'login']);
          }, 10000);
        }, error: (error) => {
          console.error('Failed to reset password');
        }
      });
  }

  email: string;
  token: string;
  password: string;
  passwordConfirmation: string;
  success: boolean;
}
