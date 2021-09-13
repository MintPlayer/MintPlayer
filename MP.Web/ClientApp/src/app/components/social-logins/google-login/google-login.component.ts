import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { API_VERSION } from '@mintplayer/ng-client';
import { BaseLoginComponent } from '../base-login.component';
import { PwaHelper } from '../../../helpers/pwa.helper';
import { EXTERNAL_URL } from '../../../providers/external-url.provider';

@Component({
  selector: 'app-google-login',
  templateUrl: './google-login.component.html',
  styleUrls: ['./google-login.component.scss']
})
export class GoogleLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(EXTERNAL_URL) externalUrl: string,
    @Inject(API_VERSION) apiVersion: string,
    pwaHelper: PwaHelper,
  ) {
    super(externalUrl, 'Google', pwaHelper, apiVersion);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }

}
