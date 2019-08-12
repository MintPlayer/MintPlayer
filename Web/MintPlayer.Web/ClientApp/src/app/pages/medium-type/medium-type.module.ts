import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MediumTypeRoutingModule } from './medium-type-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { ControlsModule } from '../../controls/controls.module';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    EditComponent,
    ShowComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    MediumTypeRoutingModule
  ]
})
export class MediumTypeModule { }
