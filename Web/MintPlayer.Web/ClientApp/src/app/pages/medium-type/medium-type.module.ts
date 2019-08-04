import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MediumTypeRoutingModule } from './medium-type-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';


@NgModule({
  declarations: [ListComponent, CreateComponent, EditComponent, ShowComponent],
  imports: [
    CommonModule,
    MediumTypeRoutingModule
  ]
})
export class MediumTypeModule { }
