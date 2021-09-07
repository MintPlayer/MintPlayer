import { HttpClient } from '@angular/common/http';
import { ElementRef, ViewChild } from '@angular/core';
import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { AccountService, API_VERSION, LoginResult, User } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BehaviorSubject } from 'rxjs';
import { Subject } from 'rxjs';
import { filter, map, take, takeUntil, takeWhile } from 'rxjs/operators';
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
      this.accountService.csrfRefresh().then(() => {
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
        });
        this.httpClient.post<TwoFactorRegistrationUrl>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-registration`, {})
          .subscribe((urlData) => {
            this.twoFaRegistrationUrl = urlData.registrationUrl;
            this.twoFaRegistrationUrlSanitized = this.domSanitizer.bypassSecurityTrustUrl(this.twoFaRegistrationUrl);
          });
        this.httpClient.get<number>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-recovery-remaining-codes`)
          .subscribe((remainingNumberOfCodes) => {
            this.numberOfRecoveryCodesLeft = remainingNumberOfCodes;
          });
      });
    }

    this.isNotRequestingVerificationCode$
      .pipe(takeUntil(this.destroyed$))
      .pipe(filter(r => !r))
      .subscribe((r) => {
        this.verificationCode = '';
        setTimeout(() => {
          if (!!this.txtVerificationCode) {
            this.txtVerificationCode.nativeElement.focus();
          }
        }, 20);
      });
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
  numberOfRecoveryCodesLeft: number | null = null;

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
      this.accountService.updatePassword(this.currentPassword, this.newPassword, this.passwordConfirmation).then(() => {
        this.router.navigate(['/']);
      }).catch((error) => {
        console.error(error);
      });
    }
  }

  verificationCode: string = '';
  isNotRequestingVerificationCode$ = new Subject<boolean>();
  private twoFactorCodeEntered$ = new Subject<string>();
  onTwoFactorCodeEntered(code: string | null) {
    if (code === null) {
      this.isNotRequestingVerificationCode$.next(true);
    } else {
      this.twoFactorCodeEntered$.next(code);
    }
  }

  setEnableTwoFactor(enable: boolean) {
    console.log('set isNotRequestingVerificationCode$ to false');
    this.isNotRequestingVerificationCode$.next(false);
    this.twoFactorCodeEntered$
      .pipe(takeUntil(this.isNotRequestingVerificationCode$), takeUntil(this.destroyed$))
      .subscribe((code) => {
        this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-setup`, { setupCode: code, enabled: enable })
          .subscribe((backupCodes) => {
            this.backupCodes = backupCodes;
            this.user.isTwoFactorEnabled = enable;
            this.isNotRequestingVerificationCode$.next(true);
          }, () => {
            console.log('wrong code');
          });
      });
    return false;
  }

  setBypass2faForExternalLogin(enable: boolean) {
    this.isNotRequestingVerificationCode$.next(false);
    this.twoFactorCodeEntered$
      .pipe(takeUntil(this.isNotRequestingVerificationCode$), takeUntil(this.destroyed$))
      .subscribe((code) => {
        this.accountService.setBypass2faForExternalLogin(code, enable).then(() => {
          this.user.bypass2faForExternalLogin = enable;
          this.isNotRequestingVerificationCode$.next(true);
        });
      }, () => {
        console.log('Wrong code');
      });
    return false;
  }

  generateNewRecoveryCodes() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-generate-new-codes`, null)
      .subscribe((backupCodes) => {
        this.backupCodes = backupCodes;
      });
  }

  private destroyed$ = new Subject();
  @ViewChild('txt_verification_code') txtVerificationCode: ElementRef<HTMLInputElement>;

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.destroyed$.next(true);
  }
}
