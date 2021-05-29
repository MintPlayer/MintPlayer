import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JsonLdModule } from '@mintplayer/ng-json-ld';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { ComponentsModule } from '../../../components/components.module';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    JsonLdModule,
    ControlsModule,
    ComponentsModule,
    PipesModule,
    AdvancedRouterModule,

    ShowRoutingModule
  ]
})
export class ShowModule { }
