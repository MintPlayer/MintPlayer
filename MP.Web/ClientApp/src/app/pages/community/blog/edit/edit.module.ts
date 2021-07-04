import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { CardModule } from '../../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../../directives/for/for-directive.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,

    CardModule,
    ForDirectiveModule,
    EditRoutingModule
  ]
})
export class EditModule { }
