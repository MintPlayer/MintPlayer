import { Component, OnInit } from '@angular/core';
import { LoginResult } from '../../../interfaces/loginResult';
import { AccountService } from '../../../services/account/account.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  constructor(private accountService: AccountService) {
    this.accountService.getLogins().subscribe((logins) => {
      this.userLogins = logins;
    });
    this.accountService.getProviders().subscribe((providers) => {
      this.loginProviders = providers;
    });
  }

  loginProviders: string[] = [];
  userLogins: string[] = [];
  errorMessage: string = null;

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.errorMessage = null;
      this.accountService.getLogins().subscribe((logins) => {
        this.userLogins = logins;
      });
    } else {
      this.errorMessage = result.errorDescription;
    }
  }

  removeSocialLogin(provider: string) {
    this.accountService.removeLogin(provider).subscribe(() => {
      this.userLogins.splice(this.userLogins.indexOf(provider), 1);
    });
  }

  ngOnInit() {
  }

}
