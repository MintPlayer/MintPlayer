import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { ControlsModule } from '../../../controls/controls.module';



@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    DatatablesModule,
    AdvancedRouterModule,
    ControlsModule,

    ListRoutingModule
  ]
})
export class ListModule { }
