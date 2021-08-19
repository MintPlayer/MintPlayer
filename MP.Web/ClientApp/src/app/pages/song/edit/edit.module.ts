import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Select2Module } from '@mintplayer/ng-select2';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { CardModule } from '../../../controls/card/card.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { MediaManagerModule } from '../../../components/subject/media-manager/media-manager.module';
import { focusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';



@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    Select2Module,

    CardModule,
    PipesModule,
    ForDirectiveModule,
    focusOnLoadDirectiveModule,
    MediaManagerModule,

    EditRoutingModule
  ]
})
export class EditModule { }
