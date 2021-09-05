import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { API_VERSION } from '@mintplayer/ng-client';
import { PwaHelper } from '../../../helpers/pwa.helper';
import { EXTERNAL_URL } from '../../../providers/external-url.provider';
import { BaseLoginComponent } from '../base-login.component';

@Component({
  selector: 'app-linkedin-login',
  templateUrl: './linkedin-login.component.html',
  styleUrls: ['./linkedin-login.component.scss']
})
export class LinkedinLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {

  constructor(@Inject(EXTERNAL_URL) externalUrl: string, @Inject(API_VERSION) apiVersion: string, pwaHelper: PwaHelper) {
    super(externalUrl, 'LinkedIn', pwaHelper, apiVersion);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }

}
