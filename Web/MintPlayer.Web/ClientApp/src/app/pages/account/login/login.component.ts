import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AccountService } from '../../../services/account/account.service';
import { LoginResult } from '../../../interfaces/loginResult';
import { HttpErrorResponse } from '@angular/common/http';
import { User } from '../../../interfaces/account/user';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  constructor(private accountService: AccountService, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
  }

  private returnUrl: string = '';

  email: string;
  password: string;
  loginResult: LoginResult = {
    status: true,
    platform: 'local',
    user: null,
    token: null,
    error: null,
    errorDescription: null
  };

  login() {
    this.accountService.login(this.email, this.password).subscribe((result) => {
      if (result.status === true) {
        this.router.navigateByUrl(this.returnUrl);
        this.loginComplete.emit(result.user);
      } else {
        this.loginResult = result;
      }
    }, (error: HttpErrorResponse) => {
      this.loginResult = {
        status: false,
        platform: 'local',
        user: null,
        token: null,
        error: 'Login attempt failed',
        errorDescription: 'Check your connection'
      };
    });
  }

  socialLoginDone(result: LoginResult) {
    if (result.status) {
      this.accountService.currentUser().subscribe((user) => {
        this.loginComplete.emit(user);
        this.router.navigateByUrl(this.returnUrl);
      });
    } else {
      this.loginResult = result;
    }
  }

  @Output() loginComplete: EventEmitter<User> = new EventEmitter();

  forgotPassword() {
  }

}
