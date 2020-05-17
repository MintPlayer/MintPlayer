import { Component, OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../services/account/account.service';
import { LoginResult } from '../../../entities/login-result';
import { User } from '../../../entities/user';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  constructor(private accountService: AccountService, private router: Router, private route: ActivatedRoute, private htmlLink: HtmlLinkHelper) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

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
        this.router.navigateByUrl(this.returnUrl);
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
        this.router.navigateByUrl(this.returnUrl);
      });
    } else {
      this.loginResult = result;
    }
  }

  forgotPassword() {
  }

  @Output() loginComplete: EventEmitter<User> = new EventEmitter();
}
