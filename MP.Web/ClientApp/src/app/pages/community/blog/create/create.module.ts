import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { CardModule } from '../../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../../directives/for/for-directive.module';


@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,

    CardModule,
    ForDirectiveModule,
    CreateRoutingModule
  ]
})
export class CreateModule { }
