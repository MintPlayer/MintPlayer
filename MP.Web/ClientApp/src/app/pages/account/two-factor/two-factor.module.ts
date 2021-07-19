import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { TwoFactorRoutingModule } from './two-factor-routing.module';
import { TwoFactorComponent } from './two-factor.component';
import { AutofocusDirectiveModule } from '../../../directives/autofocus/autofocus-directive.module';


@NgModule({
  declarations: [
    TwoFactorComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    AutofocusDirectiveModule,
    TwoFactorRoutingModule
  ]
})
export class TwoFactorModule { }
