import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
