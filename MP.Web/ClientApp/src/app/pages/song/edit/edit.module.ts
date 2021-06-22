import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Select2Module } from '@mintplayer/ng-select2';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { ComponentsModule } from '../../../components/components.module';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    Select2Module,
    ControlsModule,
    ComponentsModule,
    DirectivesModule,

    PipesModule,
    EditRoutingModule
  ]
})
export class EditModule { }
