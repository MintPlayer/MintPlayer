import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NotFoundRoutingModule } from './not-found-routing.module';
import { NotFoundComponent } from './not-found.component';
import { DirectivesModule } from '../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [NotFoundComponent],
  imports: [
    CommonModule,
    QueryParamsHandlingModule,
    NotFoundRoutingModule
  ]
})
export class NotFoundModule { }
