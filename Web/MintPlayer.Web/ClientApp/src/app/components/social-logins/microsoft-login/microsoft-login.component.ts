import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { BaseLoginComponent } from '../base-login-component';

@Component({
  selector: 'app-microsoft-login',
  templateUrl: './microsoft-login.component.html',
  styleUrls: ['./microsoft-login.component.scss']
})
export class MicrosoftLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {

  constructor(@Inject('BASE_URL') baseUrl: string) {
    super(baseUrl, "Microsoft");
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }

}
