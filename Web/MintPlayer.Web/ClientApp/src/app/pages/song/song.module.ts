import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { SongRoutingModule } from './song-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { ControlsModule } from '../../controls/controls.module';
import { ComponentsModule } from '../../components/components.module';
import { SubjectModule } from '../../components/subject/subject.module';


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
    SubjectModule,
    ComponentsModule,
    SongRoutingModule
  ]
})
export class SongModule { }
