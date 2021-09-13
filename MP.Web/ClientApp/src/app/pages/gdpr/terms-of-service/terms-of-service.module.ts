import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { TermsOfServiceRoutingModule } from './terms-of-service-routing.module';
import { TermsOfServiceComponent } from './terms-of-service.component';



@NgModule({
  declarations: [TermsOfServiceComponent],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,
    TermsOfServiceRoutingModule
  ]
})
export class TermsOfServiceModule { }
