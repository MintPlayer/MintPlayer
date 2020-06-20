import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { PipesModule } from '../../../../pipes/pipes.module';


@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    PipesModule,
    ListRoutingModule
  ]
})
export class ListModule { }
