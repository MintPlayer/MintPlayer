import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SongRoutingModule } from './song-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';


@NgModule({
  declarations: [ListComponent, CreateComponent, EditComponent, ShowComponent],
  imports: [
    CommonModule,
    SongRoutingModule
  ]
})
export class SongModule { }