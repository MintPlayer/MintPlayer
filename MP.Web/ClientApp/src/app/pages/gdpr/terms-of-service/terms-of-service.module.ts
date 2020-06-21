import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TermsOfServiceRoutingModule } from './terms-of-service-routing.module';
import { TermsOfServiceComponent } from './terms-of-service.component';
import { DirectivesModule } from '../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [TermsOfServiceComponent],
  imports: [
    CommonModule,
    QueryParamsHandlingModule,
    TermsOfServiceRoutingModule
  ]
})
export class TermsOfServiceModule { }
