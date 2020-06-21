import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../../pipes/pipes.module';
import { DirectivesModule } from '../../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    NgxJsonLdModule,
    PipesModule,
    QueryParamsHandlingModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
