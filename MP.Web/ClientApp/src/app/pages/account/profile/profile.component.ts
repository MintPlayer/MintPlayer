import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { AccountService, LoginResult } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
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
    private router: AdvancedRouter,
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
      this.accountService.hasPassword().then((hasPassword) => {
        this.hasPassword = hasPassword;
      });
    }
  }

  hasPassword: boolean | null = null;
  currentPassword: string = null;
  newPassword: string = null;
  passwordConfirmation: string = null;
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

  updateProfile() {
    if (this.newPassword !== '') {
      this.accountService.updatePassword(this.currentPassword, this.newPassword, this.passwordConfirmation)
        .then(() => {
          this.router.navigate(['/']);
        });
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
