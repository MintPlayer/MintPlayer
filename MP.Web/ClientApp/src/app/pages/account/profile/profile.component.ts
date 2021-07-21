import { HttpClient } from '@angular/common/http';
import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { AccountService, API_VERSION, LoginResult, User } from '@mintplayer/ng-client';
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
    private domSanitizer: DomSanitizer,

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
      this.accountService.currentUser().then((user) => {
        this.user = user;
        console.log('user', this.user);
      });
      this.httpClient.post<TwoFactorRegistrationUrl>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-registration`, {})
        .subscribe((urlData) => {
          this.twoFaRegistrationUrl = urlData.registrationUrl;
          this.twoFaRegistrationUrlSanitized = this.domSanitizer.bypassSecurityTrustUrl(this.twoFaRegistrationUrl);
        });
    }
  }

  user: User = null;
  hasPassword: boolean | null = null;
  currentPassword: string = null;
  newPassword: string = null;
  passwordConfirmation: string = null;
  loginProviders: string[] = [];
  userLogins: string[] = [];

  twoFaRegistrationUrl: string = null;
  twoFaRegistrationUrlSanitized: SafeUrl = null;

  backupCodes: string[] = null;
  isRegistrationSuccess: boolean = false;

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

  updatePassword() {
    if (this.newPassword !== '') {
      this.accountService.updatePassword(this.currentPassword, this.newPassword, this.passwordConfirmation)
        .then(() => {
          this.router.navigate(['/']);
        });
    }
  }

  enableVerificationCode: string = '';
  finshTwoFactorSetup() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-setup`, { setupCode: this.enableVerificationCode })
      .subscribe((backupCodes) => {
        this.backupCodes = backupCodes;
        this.isRegistrationSuccess = true;
        this.user.isTwoFactorEnabled = true;
      });

    return false;
  }

  disableVerificationCode: string = '';
  disableTwoFactor() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-disable`, { setupCode: this.disableVerificationCode })
      .subscribe(() => {
        this.user.isTwoFactorEnabled = false;
      });

    return false;
  }

  setBypass2faForExternalLogin(enable: boolean) {
    this.accountService.setBypass2faForExternalLogin(this.disableVerificationCode, enable)
      .then(() => {
        this.user.bypass2faForExternalLogin = enable;
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
