import { Component, OnInit, Inject } from '@angular/core';
import { BaseLoginComponent } from '../base-login-component';

@Component({
  selector: 'app-facebook-login',
  templateUrl: './facebook-login.component.html',
  styleUrls: ['./facebook-login.component.scss']
})
export class FacebookLoginComponent extends BaseLoginComponent implements OnInit {
  constructor(@Inject('BASE_URL') baseUrl: string) {
    super(baseUrl, 'Facebook');
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }
}
