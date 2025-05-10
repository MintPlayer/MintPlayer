import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { Subject } from 'rxjs';
import { AccountService, LoginResult, User } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { ELoginStatus } from '@mintplayer/ng-client';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  constructor(
    private accountService: AccountService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta,
  ) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private hreflangTags: HTMLLinkElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: 'Welcome on the login page of MintPlayer.'
    }]);
  }
  private addOpenGraphTags() {

  }
  private addTwitterCard() {

  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.hreflangTags !== null) {
      this.hreflangTags.forEach((tag) => {
        tag.parentNode.removeChild(tag);
      });
    }
  }
  //#endregion

  email: string;
  password: string;
  unconfirmedEmail: boolean;
  private returnUrl: string = '';
  loginStatuses = ELoginStatus;
  loginResult: LoginResult = {
    status: ELoginStatus.success,
    medium: '',
    platform: 'local',
    user: null,
    error: null,
    errorDescription: null,
  };

  login() {
    this.accountService.login(this.email, this.password).subscribe({
      next: (loginResult) => {
        this.accountService.csrfRefresh().subscribe({
          next: () => {
            switch (loginResult.status) {
              case ELoginStatus.success: {
                this.router.navigateByUrl(this.returnUrl);
                this.loginComplete.next(loginResult.user);
              } break;
              case ELoginStatus.requiresTwoFactor: {
                this.router.navigate(['/account', 'two-factor'], { queryParams: { return: this.returnUrl } });
              } break;
              default: {
                this.loginResult = loginResult;
              } break;
            }
          }
        });
      },
      error: (error: HttpErrorResponse) => {
        switch (error.status) {
          case 403: {
            this.loginResult = {
              status: ELoginStatus.failed,
              medium: '',
              platform: 'local',
              user: null,
              error: 'Confirm email address',
              errorDescription: 'Your email address is not confirmed. If you don\'t have a confirmation email, click the button below to re-send one.'
            };
            this.unconfirmedEmail = true;
          } break;
          default: {
            this.loginResult = {
              status: ELoginStatus.failed,
              medium: '',
              platform: 'local',
              user: null,
              error: 'Login attempt failed',
              errorDescription: 'Check your connection'
            };
            this.unconfirmedEmail = false;
          } break;
        }
      }
    });
  }

  resendConfirmationEmail() {
    this.accountService.resendConfirmationEmail(this.email).subscribe({
      next: () => {
        this.unconfirmedEmail = false;
      },
      error: (error) => {
        this.loginResult = {
          status: ELoginStatus.success,
          medium: '',
          platform: 'local',
          user: null,
          error: 'Could not send confirmation email',
          errorDescription: 'Something went wrong while sending the confirmation email'
        };
      }
    });
    return false;
  }

  socialLoginDone(result: LoginResult) {
    switch (result.status) {
      case ELoginStatus.success: {
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
        break;
      }
      case ELoginStatus.requiresTwoFactor: {
        this.router.navigate(['/account', 'two-factor'], { queryParams: { return: this.returnUrl } });
        break;
      }
      default: {
        this.loginResult = result;
        break;
      }
    }
  }

  loginComplete: Subject<User> = new Subject<User>();
}
