import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { PipesModule } from '../../../../pipes/pipes.module';
import { DirectivesModule } from '../../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    PipesModule,
    QueryParamsHandlingModule,
    ListRoutingModule
  ]
})
export class ListModule { }
