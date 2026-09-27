import { HttpClient } from '@angular/common/http';
import { ElementRef, ViewChild } from '@angular/core';
import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { AccountService, MINTPLAYER_API_VERSION, LoginResult, User } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BehaviorSubject } from 'rxjs';
import { Subject } from 'rxjs';
import { filter, map, take, takeUntil, takeWhile } from 'rxjs/operators';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { TwoFactorRegistrationUrl } from '../../../interfaces/two-factor-registration-url';
import { WebAuthnService } from '../../../services/webauthn/webauthn.service';
import { WebAuthnCredentialInfo } from '../../../interfaces/webauthn-credential-info';

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
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
    private webAuthnService: WebAuthnService,
  ) {
    // Check if WebAuthn is supported
    this.webAuthnSupported = this.webAuthnService.isSupported();
    if (serverSide === true) {
      this.userLogins = loginsInj;
      this.loginProviders = providersInj;
    } else {
      this.accountService.csrfRefresh().subscribe({
        next: () => {
          this.accountService.getLogins().subscribe({
            next: (logins) => {
              this.userLogins = logins;
            }
          });
          this.accountService.getProviders().subscribe({
            next: (providers) => {
              this.loginProviders = providers;
            }
          });
          this.accountService.hasPassword().subscribe({
            next: (hasPassword) => {
              this.hasPassword = hasPassword;
            }
          });
          this.accountService.currentUser().subscribe({
            next: (user) => {
              this.user = user;
            }
          });
          this.httpClient.post<TwoFactorRegistrationUrl>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-registration`, {})
            .subscribe({
              next: (urlData) => {
                this.twoFaRegistrationUrl = urlData.registrationUrl;
                this.twoFaRegistrationUrlSanitized = this.domSanitizer.bypassSecurityTrustUrl(this.twoFaRegistrationUrl);
              }
            });
          this.httpClient.get<number>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-recovery-remaining-codes`)
            .subscribe({
              next: (remainingNumberOfCodes) => {
                this.numberOfRecoveryCodesLeft = remainingNumberOfCodes;
              }
            });
          // Load passkeys
          if (this.webAuthnSupported) {
            this.loadPasskeys();
          }
        }
      });
    }

    this.isNotRequestingVerificationCode$
      .pipe(takeUntil(this.destroyed$))
      .pipe(filter(r => !r))
      .subscribe({
        next: (r) => {
          this.verificationCode = '';
          setTimeout(() => {
            if (!!this.txtVerificationCode) {
              this.txtVerificationCode.nativeElement.focus();
            }
          }, 20);
        }
      });
  }

  baseUrl = this.baseUrlService.getBaseUrl();
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

  // Passkey properties
  webAuthnSupported: boolean = false;
  passkeys: WebAuthnCredentialInfo[] = [];
  newPasskeyName: string = '';
  isRegisteringPasskey: boolean = false;
  passkeyError: string = null;

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.accountService.getLogins().subscribe({
        next: (logins) => {
          this.userLogins = logins;
        }
      });
    } else {
    }
  }

  removeSocialLogin(provider: string) {
    this.accountService.removeLogin(provider).subscribe({
      next: () => {
        this.userLogins.splice(this.userLogins.indexOf(provider), 1);
      }
    });
  }

  updatePassword() {
    if (this.newPassword !== '') {
      this.accountService.updatePassword(this.currentPassword, this.newPassword, this.passwordConfirmation)
        .subscribe({
          next: () => {
            this.router.navigate(['/']);
          }, error: (error) => {
            console.error(error);
          }
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
          .subscribe({
            next: (backupCodes) => {
              this.backupCodes = backupCodes;
              this.user.isTwoFactorEnabled = enable;
              this.isNotRequestingVerificationCode$.next(true);
            }, error: () => {
              console.log('wrong code');
            }
          });
      });
    return false;
  }

  setBypass2faForExternalLogin(enable: boolean) {
    this.isNotRequestingVerificationCode$.next(false);
    this.twoFactorCodeEntered$
      .pipe(takeUntil(this.isNotRequestingVerificationCode$), takeUntil(this.destroyed$))
      .subscribe({
        next: (code) => {
          this.accountService.setBypass2faForExternalLogin(code, enable).subscribe({
            next: () => {
              this.user.bypass2faForExternalLogin = enable;
              this.isNotRequestingVerificationCode$.next(true);
            }
          });
        }, error: () => {
          console.log('Wrong code');
        }
      });
    return false;
  }

  generateNewRecoveryCodes() {
    this.httpClient.post<string[]>(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-generate-new-codes`, null)
      .subscribe({
        next: (backupCodes) => {
          this.backupCodes = backupCodes;
        }
      });
  }

  // Passkey methods
  loadPasskeys() {
    this.webAuthnService.getCredentials().subscribe({
      next: (credentials) => {
        this.passkeys = credentials;
      },
      error: (error) => {
        console.error('Failed to load passkeys:', error);
      }
    });
  }

  registerPasskey() {
    this.passkeyError = null;
    this.isRegisteringPasskey = true;
    const displayName = this.newPasskeyName || 'Passkey';

    this.webAuthnService.registerPasskey(displayName).subscribe({
      next: (credential) => {
        this.passkeys.push(credential);
        this.newPasskeyName = '';
        this.isRegisteringPasskey = false;
      },
      error: (error) => {
        console.error('Failed to register passkey:', error);
        this.passkeyError = error.message || 'Failed to register passkey. Please try again.';
        this.isRegisteringPasskey = false;
      }
    });
  }

  removePasskey(id: number) {
    if (confirm('Are you sure you want to remove this passkey?')) {
      this.webAuthnService.removeCredential(id).subscribe({
        next: () => {
          this.passkeys = this.passkeys.filter(p => p.id !== id);
        },
        error: (error) => {
          console.error('Failed to remove passkey:', error);
          this.passkeyError = 'Failed to remove passkey. Please try again.';
        }
      });
    }
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
