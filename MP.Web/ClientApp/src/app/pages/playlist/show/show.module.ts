import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
//import { JsonLdModule } from '@mintplayer/ng-json-ld';

import { ShowRoutingModule } from './show-routing.module';
import { PlaylistShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PlaylistShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,

    //JsonLdModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
