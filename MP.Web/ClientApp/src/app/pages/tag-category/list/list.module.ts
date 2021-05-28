import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    ControlsModule,

    ListRoutingModule
  ]
})
export class ListModule { }
