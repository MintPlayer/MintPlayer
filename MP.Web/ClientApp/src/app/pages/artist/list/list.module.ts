import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    ControlsModule,
    QueryParamsHandlingModule, // For language queryparam
    PipesModule,
    ListRoutingModule
  ]
})
export class ListModule { }
