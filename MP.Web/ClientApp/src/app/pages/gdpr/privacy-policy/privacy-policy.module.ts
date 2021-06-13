import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { PrivacyPolicyRoutingModule } from './privacy-policy-routing.module';
import { PrivacyPolicyComponent } from './privacy-policy.component';



@NgModule({
  declarations: [PrivacyPolicyComponent],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,
    PrivacyPolicyRoutingModule
  ]
})
export class PrivacyPolicyModule { }
