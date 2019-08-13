import { Component, OnInit, Inject } from '@angular/core';
import { BaseLoginComponent } from '../base-login-component';

@Component({
  selector: 'app-google-login',
  templateUrl: './google-login.component.html',
  styleUrls: ['./google-login.component.scss']
})
export class GoogleLoginComponent extends BaseLoginComponent implements OnInit {

  constructor(@Inject('BASE_URL') protected baseUrl: string) {
    super(baseUrl, "Google");
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }

}
