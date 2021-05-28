import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';

import { PublicRoutingModule } from './public-routing.module';
import { PlaylistPublicComponent } from './public.component';


@NgModule({
  declarations: [PlaylistPublicComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,

    PublicRoutingModule
  ]
})
export class PublicModule { }
