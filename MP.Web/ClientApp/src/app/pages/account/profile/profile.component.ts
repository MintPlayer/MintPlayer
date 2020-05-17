import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { AccountService } from '../../../services/account/account.service';
import { LoginResult } from '../../../entities/login-result';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  constructor(@Inject('LOGINS') private loginsInj: string[], @Inject('PROVIDERS') private providersInj: string[], private accountService: AccountService, private htmlLink: HtmlLinkHelper) {
    if (loginsInj === null) {
      this.accountService.getLogins().then((logins) => {
        this.userLogins = logins;
      });
    } else {
      this.userLogins = loginsInj;
    }

    if (providersInj === null) {
      this.accountService.getProviders().then((providers) => {
        this.loginProviders = providers;
      });
    } else {
      this.loginProviders = providersInj;
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
