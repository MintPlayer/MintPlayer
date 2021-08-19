import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { focusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';



@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,

    CardModule,
    ForDirectiveModule,
    focusOnLoadDirectiveModule,
    CreateRoutingModule
  ]
})
export class CreateModule { }
