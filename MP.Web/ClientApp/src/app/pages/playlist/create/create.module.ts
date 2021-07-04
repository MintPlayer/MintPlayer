import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';

import { CreateRoutingModule } from './create-routing.module';
import { PlaylistCreateComponent } from './create.component';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PlaylistCreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    DragDropModule,
    DirectivesModule,

    CreateRoutingModule
  ]
})
export class CreateModule { }
