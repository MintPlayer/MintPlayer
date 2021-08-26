import { HttpClient } from '@angular/common/http';
import { Inject } from '@angular/core';
import { AfterViewInit } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { API_VERSION } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';

@Component({
  selector: 'app-perform',
  templateUrl: './perform.component.html',
  styleUrls: ['./perform.component.scss']
})
export class PerformComponent implements OnInit, AfterViewInit {

  constructor(private router: AdvancedRouter, private route: ActivatedRoute, private httpClient: HttpClient, @Inject(BASE_URL) private baseUrl: string, @Inject(API_VERSION) private apiVersion: string) {
  }

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
      .toPromise()
      .then((response) => {
        this.success = true;
        setTimeout(() => {
          this.router.navigate(['/account', 'login']);
        }, 10000);
      }).catch((error) => {
        console.error('Failed to reset password');
      });
  }

  email: string;
  token: string;
  password: string;
  passwordConfirmation: string;
  success: boolean;
}
