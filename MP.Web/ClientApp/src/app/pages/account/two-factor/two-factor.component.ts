import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { AccountService, API_VERSION, User } from '@mintplayer/ng-client';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-two-factor',
  templateUrl: './two-factor.component.html',
  styleUrls: ['./two-factor.component.scss']
})
export class TwoFactorComponent implements OnInit {

  constructor(
    private httpClient: HttpClient,
    private accountService: AccountService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    @Inject(BASE_URL) private baseUrl: string,
    @Inject(API_VERSION) private apiVersion: string
  ) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
  }

  setupCode: string = '';
  private returnUrl: string = '';

  verifyCode() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-login`, { code: this.setupCode, remember: false }).subscribe((backupCodes) => {
      this.accountService.currentUser().then((user) => {
        this.loginComplete.next(user);
        this.router.navigateByUrl(this.returnUrl);
      });
    });
    return false;
  }

  loginComplete: Subject<User> = new Subject<User>();
}
