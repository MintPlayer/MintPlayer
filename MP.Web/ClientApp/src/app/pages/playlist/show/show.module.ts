import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { ShowRoutingModule } from './show-routing.module';
import { PlaylistShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [PlaylistShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,
    QueryParamsHandlingModule,
    NgxJsonLdModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
