import { Component, OnInit, Inject } from '@angular/core';
import { API_VERSION } from '@mintplayer/ng-client';
import { BaseLoginComponent } from '../base-login.component';
import { PwaHelper } from '../../../helpers/pwa.helper';
import { EXTERNAL_URL } from '../../../providers/external-url.provider';

@Component({
  selector: 'app-microsoft-login',
  templateUrl: './microsoft-login.component.html',
  styleUrls: ['./microsoft-login.component.scss']
})
export class MicrosoftLoginComponent extends BaseLoginComponent implements OnInit {
  constructor(
    @Inject(EXTERNAL_URL) externalUrl: string,
    @Inject(API_VERSION) apiVersion: string,
    pwaHelper: PwaHelper,
  ) {
    super(externalUrl, 'Microsoft', pwaHelper, apiVersion);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }
}
