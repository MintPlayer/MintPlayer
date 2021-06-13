import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';

import { PublicRoutingModule } from './public-routing.module';
import { PlaylistPublicComponent } from './public.component';


@NgModule({
  declarations: [PlaylistPublicComponent],
  imports: [
    CommonModule,
    DatatablesModule,
    ControlsModule,
    PipesModule,
    AdvancedRouterModule,

    PublicRoutingModule
  ]
})
export class PublicModule { }
