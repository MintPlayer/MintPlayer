import { HttpClient } from '@angular/common/http';
import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { AccountService, API_VERSION, LoginResult } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { TwoFactorRegistrationUrl } from '../../../interfaces/two-factor-registration-url';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('LOGINS') private loginsInj: string[],
    @Inject('PROVIDERS') private providersInj: string[],
    private router: AdvancedRouter,
    private accountService: AccountService,
    private htmlLink: HtmlLinkHelper,

    private httpClient: HttpClient,
    @Inject(BASE_URL) private baseUrl: string,
    @Inject(API_VERSION) private apiVersion: string
  ) {
    if (serverSide === true) {
      this.userLogins = loginsInj;
      this.loginProviders = providersInj;
    } else {
      this.accountService.getLogins().then((logins) => {
        this.userLogins = logins;
      });
      this.accountService.getProviders().then((providers) => {
        this.loginProviders = providers;
      });
      this.accountService.hasPassword().then((hasPassword) => {
        this.hasPassword = hasPassword;
      });
      this.httpClient.post<TwoFactorRegistrationUrl>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-registration`, {})
        .subscribe((urlData) => {
          console.log('Set twoFaRegistrationUrl', urlData.registrationUrl);
          this.twoFaRegistrationUrl = urlData.registrationUrl;
        });
    }
  }

  hasPassword: boolean | null = null;
  currentPassword: string = null;
  newPassword: string = null;
  passwordConfirmation: string = null;
  loginProviders: string[] = [];
  userLogins: string[] = [];

  is2FaEnabled: boolean = false;
  twoFaRegistrationUrl: string = null;
  verificationCode: string = '';
  backupCodes: string[] = [];

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.accountService.getLogins().then((logins) => {
        this.userLogins = logins;
      });
    } else {
    }
  }

  removeSocialLogin(provider: string) {
    this.accountService.removeLogin(provider).then(() => {
      this.userLogins.splice(this.userLogins.indexOf(provider), 1);
    });
  }

  updateProfile() {
    if (this.newPassword !== '') {
      this.accountService.updatePassword(this.currentPassword, this.newPassword, this.passwordConfirmation)
        .then(() => {
          this.router.navigate(['/']);
        });
    }
  }

  change2FactorAuthentication(enable: boolean) {
    console.log(enable ? 'Enable 2-factor authentication' : 'Disable 2-factor authentication');

  }

  finshTwoFactorSetup() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-setup`, { setupCode: this.verificationCode })
      .subscribe((backupCodes) => {
        this.backupCodes = backupCodes;
      });

    return false;
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
