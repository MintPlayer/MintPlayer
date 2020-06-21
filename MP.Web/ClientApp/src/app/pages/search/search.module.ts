import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { SearchRoutingModule } from './search-routing.module';
import { SearchComponent } from './search.component';
import { ControlsModule } from '../../controls/controls.module';
import { PipesModule } from '../../pipes/pipes.module';
import { DirectivesModule } from '../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [SearchComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    PipesModule,
    QueryParamsHandlingModule,
    TranslateModule,
    SearchRoutingModule
  ]
})
export class SearchModule { }
