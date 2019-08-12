import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../services/account/account.service';
import { UserData } from '../../../interfaces/account/userData';
import { User } from '../../../interfaces/account/user';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  constructor(private accountService: AccountService, private router: Router) {
  }

  public data: UserData = {
    user: {
      id: '',
      userName: '',
      email: '',
      pictureUrl: ''
    },
    password: '',
    passwordConfirmation: ''
  };

  ngOnInit() {
  }

  public register() {
    this.accountService.register(this.data).subscribe((result) => {
      this.accountService.login(
        this.data.user.email, this.data.password
      ).subscribe((login_result) => {
        if (login_result.status === true) {
          this.router.navigate(['/']);
          this.loginComplete.emit(login_result.user);
        } else {
          debugger;
        }
      });
    }, (error: HttpErrorResponse) => {
      debugger;
    });
  }

  @Output() loginComplete: EventEmitter<User> = new EventEmitter();
}
