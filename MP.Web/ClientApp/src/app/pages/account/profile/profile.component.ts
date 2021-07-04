import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { AccountService, LoginResult } from '@mintplayer/ng-client';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

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
    private accountService: AccountService,
    private htmlLink: HtmlLinkHelper
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
    }
  }

  loginProviders: string[] = [];
  userLogins: string[] = [];

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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
