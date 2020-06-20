import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { ComponentsModule } from '../../../components/components.module';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    ComponentsModule,
    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
