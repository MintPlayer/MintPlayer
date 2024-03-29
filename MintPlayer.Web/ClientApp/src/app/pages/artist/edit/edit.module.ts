import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Select2Module } from '@mintplayer/ng-select2';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { MediaManagerModule } from '../../../components/subject/media-manager/media-manager.module';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { FocusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    Select2Module,
    CardModule,
    ForDirectiveModule,
    FocusOnLoadDirectiveModule,
    MediaManagerModule,

    EditRoutingModule
  ]
})
export class EditModule { }
