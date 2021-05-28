import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PrivacyPolicyRoutingModule } from './privacy-policy-routing.module';
import { PrivacyPolicyComponent } from './privacy-policy.component';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PrivacyPolicyComponent],
  imports: [
    CommonModule,

    PrivacyPolicyRoutingModule
  ]
})
export class PrivacyPolicyModule { }
