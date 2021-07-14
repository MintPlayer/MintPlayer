import { Component, OnInit, Output, EventEmitter, OnDestroy, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { Observable, of, Subject } from 'rxjs';
import { AccountService, LoginResult, User } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

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
    this.router.navigate([], {
      queryParams: {
        return: null
      }
    });
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
  loginResult: LoginResult = {
    status: true,
    medium: '',
    platform: 'local',
    user: null,
    error: null,
    errorDescription: null,
  };

  login() {
    this.accountService.login(this.email, this.password).then((result) => {
      if (result.status === true) {
        this.router.navigateByUrl(this.returnUrl);
        this.loginComplete.next(result.user);
      } else {
        this.loginResult = result;
      }
    }).catch((error: HttpErrorResponse) => {
      switch (error.status) {
        case 403: {
          this.loginResult = {
            status: false,
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
            status: false,
            medium: '',
            platform: 'local',
            user: null,
            error: 'Login attempt failed',
            errorDescription: 'Check your connection'
          };
          this.unconfirmedEmail = false;
        } break;
      }
    });
  }

  resendConfirmationEmail() {
    this.accountService.resendConfirmationEmail(this.email).then(() => {
      this.unconfirmedEmail = false;
    }).catch((error) => {
      this.loginResult = {
        status: false,
        medium: '',
        platform: 'local',
        user: null,
        error: 'Could not send confirmation email',
        errorDescription: 'Something went wrong while sending the confirmation email'
      };
    });
    return false;
  }

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.accountService.currentUser().then((user) => {
        this.loginComplete.next(user);
        this.router.navigateByUrl(this.returnUrl);
      });
    } else {
      this.loginResult = result;
    }
  }

  forgotPassword() {
  }

  loginComplete: Subject<User> = new Subject<User>();
}
