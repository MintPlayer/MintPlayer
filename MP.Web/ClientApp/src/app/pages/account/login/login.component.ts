import { Component, OnInit, Output, EventEmitter, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Meta } from '@angular/platform-browser';
import { AccountService } from '../../../services/account/account.service';
import { LoginResult } from '../../../entities/login-result';
import { User } from '../../../entities/user';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { NavigationHelper } from '../../../helpers/navigation.helper';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  constructor(
    private accountService: AccountService,
    private navigation: NavigationHelper,
    private route: ActivatedRoute,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta
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
    this.navigation.navigate([], {
      queryParams: {
        return: null
      }
    });
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
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
  }
  //#endregion

  email: string;
  password: string;
  private returnUrl: string = '';
  loginResult: LoginResult = {
    status: true,
    medium: '',
    platform: 'local',
    user: null,
    error: null,
    errorDescription: null
  };

  login() {
    this.accountService.login(this.email, this.password).then((result) => {
      if (result.status === true) {
        this.navigation.navigateByUrl(this.returnUrl, {
          queryParamsHandling: '',
          queryParams: {
            'lang': this.route.snapshot.queryParamMap.get('lang')
          }
        });
        this.loginComplete.emit(result.user);
      } else {
        this.loginResult = result;
      }
    }).catch((error: HttpErrorResponse) => {
      this.loginResult = {
        status: false,
        medium: '',
        platform: 'local',
        user: null,
        error: 'Login attempt failed',
        errorDescription: 'Check your connection'
      };
    });
  }

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.accountService.currentUser().then((user) => {
        this.loginComplete.emit(user);
        this.navigation.navigateByUrl(this.returnUrl, {
          queryParamsHandling: '',
          queryParams: {
            'lang': this.route.snapshot.queryParamMap.get('lang')
          }
        });
      });
    } else {
      this.loginResult = result;
    }
  }

  forgotPassword() {
  }

  @Output() loginComplete: EventEmitter<User> = new EventEmitter();
}
