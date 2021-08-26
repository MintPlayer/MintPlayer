import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { RequestRoutingModule } from './request-routing.module';
import { RequestComponent } from './request.component';
import { ForDirectiveModule } from '../../../../directives/for/for-directive.module';


@NgModule({
  declarations: [
    RequestComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ForDirectiveModule,
    RequestRoutingModule
  ]
})
export class RequestModule { }
