import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';

import { EditRoutingModule } from './edit-routing.module';
import { PlaylistEditComponent } from './edit.component';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { DropdownSearchBoxModule } from '../../../controls/dropdown-search-box/dropdown-search-box.module';



@NgModule({
  declarations: [PlaylistEditComponent],
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,

    CardModule,
    ForDirectiveModule,
    DropdownSearchBoxModule,
    EditRoutingModule
  ]
})
export class EditModule { }
