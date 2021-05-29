import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ListRoutingModule } from './list-routing.module';
import { PlaylistListComponent } from './list.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [PlaylistListComponent],
  imports: [
    CommonModule,
    AdvancedRouterModule,
    ControlsModule,
    PipesModule,

    ListRoutingModule
  ]
})
export class ListModule { }
