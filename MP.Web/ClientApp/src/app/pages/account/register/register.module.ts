import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { RegisterRoutingModule } from './register-routing.module';
import { RegisterComponent } from './register.component';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';


@NgModule({
  declarations: [RegisterComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,

    ForDirectiveModule,
    RegisterRoutingModule
  ]
})
export class RegisterModule { }
