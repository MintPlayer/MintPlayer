import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { CardModule } from '../../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../../directives/for/for-directive.module';
import { FocusOnLoadDirectiveModule } from '../../../../directives/focus-on-load/focus-on-load.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,

    CardModule,
    ForDirectiveModule,
    FocusOnLoadDirectiveModule,
    EditRoutingModule
  ]
})
export class EditModule { }
