import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ListRoutingModule } from './list-routing.module';
import { PlaylistListComponent } from './list.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PlaylistListComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,

    ListRoutingModule
  ]
})
export class ListModule { }
