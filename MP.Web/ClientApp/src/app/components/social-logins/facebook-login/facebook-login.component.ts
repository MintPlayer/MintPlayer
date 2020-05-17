import { Component, OnInit, Input, Output, EventEmitter, Inject, OnDestroy } from '@angular/core';
import { LoginResult } from '../../../entities/login-result';
import { BaseLoginComponent } from '../base-login.component';
import { PwaHelper } from '../../../helpers/pwa.helper';

@Component({
  selector: 'app-facebook-login',
  templateUrl: './facebook-login.component.html',
  styleUrls: ['./facebook-login.component.scss']
})
export class FacebookLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {
  constructor(@Inject('EXTERNAL_URL') externalUrl: string, pwaHelper: PwaHelper) {
    super(externalUrl, 'Facebook', pwaHelper);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }
}
