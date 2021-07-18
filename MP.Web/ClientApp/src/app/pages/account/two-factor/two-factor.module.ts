import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { TwoFactorRoutingModule } from './two-factor-routing.module';
import { TwoFactorComponent } from './two-factor.component';


@NgModule({
  declarations: [
    TwoFactorComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    TwoFactorRoutingModule
  ]
})
export class TwoFactorModule { }
