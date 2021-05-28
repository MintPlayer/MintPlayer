import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    DirectivesModule,

    EditRoutingModule
  ]
})
export class EditModule { }
