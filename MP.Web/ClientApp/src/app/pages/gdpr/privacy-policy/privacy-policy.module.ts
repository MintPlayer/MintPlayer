import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PrivacyPolicyRoutingModule } from './privacy-policy-routing.module';
import { PrivacyPolicyComponent } from './privacy-policy.component';
import { DirectivesModule } from '../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [PrivacyPolicyComponent],
  imports: [
    CommonModule,
    QueryParamsHandlingModule,
    PrivacyPolicyRoutingModule
  ]
})
export class PrivacyPolicyModule { }
