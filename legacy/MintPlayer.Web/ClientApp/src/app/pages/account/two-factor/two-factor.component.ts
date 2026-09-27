import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { AccountService, MINTPLAYER_API_VERSION, User } from '@mintplayer/ng-client';
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
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
  ) {
  }

  baseUrl = this.baseUrlService.getBaseUrl();
  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
  }

  setupCode: string = '';
  twoFactorRememberMe: boolean = false;
  returnUrl: string = '';

  verifyCode() {
    this.httpClient.post<User>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-login`, { code: this.setupCode, remember: this.twoFactorRememberMe }).subscribe((u) => {
      this.accountService.csrfRefresh().subscribe({
        next: () => {
          this.accountService.currentUser().subscribe({
            next: (user) => {
              this.loginComplete.next(user);
              this.router.navigateByUrl(this.returnUrl);
            }
          });
        }
      });
    });
    return false;
  }

  loginComplete: Subject<User> = new Subject<User>();
}
