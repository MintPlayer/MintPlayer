import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { ComponentsModule } from '../../../components/components.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    ComponentsModule,
    PipesModule,
    QueryParamsHandlingModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
