import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { EditRoutingModule } from './edit-routing.module';
import { PlaylistEditComponent } from './edit.component';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PlaylistEditComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    DirectivesModule,

    EditRoutingModule
  ]
})
export class EditModule { }
