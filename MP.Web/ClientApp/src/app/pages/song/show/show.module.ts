import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JsonLdModule } from '@mintplayer/ng-json-ld';

import { ControlsModule } from '../../../controls/controls.module';
import { ComponentsModule } from '../../../components/components.module';
import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    JsonLdModule,
    ControlsModule,
    ComponentsModule,
    PipesModule,

    ShowRoutingModule
  ]
})
export class ShowModule { }
