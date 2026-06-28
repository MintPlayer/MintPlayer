import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { EditRoutingModule } from './edit-routing.module';
import { PlaylistEditComponent } from './edit.component';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { DropdownSearchBoxModule } from '../../../controls/dropdown-search-box/dropdown-search-box.module';
import { FocusOnLoadDirectiveModule } from '../../../directives/focus-on-load/focus-on-load.module';



@NgModule({
  declarations: [PlaylistEditComponent],
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,
    TranslateModule,

    CardModule,
    ForDirectiveModule,
    FocusOnLoadDirectiveModule,
    DropdownSearchBoxModule,
    EditRoutingModule
  ]
})
export class EditModule { }
