import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { FocusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';



@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,

    ForDirectiveModule,
    FocusOnLoadDirectiveModule,
    CreateRoutingModule
  ]
})
export class CreateModule { }
