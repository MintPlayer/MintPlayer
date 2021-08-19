import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { TwoFactorRoutingModule } from './two-factor-routing.module';
import { TwoFactorComponent } from './two-factor.component';
import { focusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';


@NgModule({
  declarations: [
    TwoFactorComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    focusOnLoadDirectiveModule,
    TwoFactorRoutingModule
  ]
})
export class TwoFactorModule { }
