import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MINTPLAYER_API_VERSION } from '@mintplayer/ng-client';
import { BaseLoginComponent } from '../base-login.component';
import { PwaHelper } from '../../../helpers/pwa.helper';
import { EXTERNAL_URL } from '../../../providers/external-url.provider';

@Component({
  selector: 'app-facebook-login',
  templateUrl: './facebook-login.component.html',
  styleUrls: ['./facebook-login.component.scss']
})
export class FacebookLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(EXTERNAL_URL) externalUrl: string,
    @Inject(MINTPLAYER_API_VERSION) apiVersion: string,
    pwaHelper: PwaHelper,
  ) {
    super(externalUrl, 'Facebook', pwaHelper, apiVersion);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }
}
