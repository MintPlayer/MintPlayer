import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { SearchRoutingModule } from './search-routing.module';
import { SearchComponent } from './search.component';
import { AutocompleteModule } from '../../controls/autocomplete/autocomplete.module';
import { PipesModule } from '../../pipes/pipes.module';
import { FocusOnLoadDirectiveModule } from '../../directives/focus-on-load/focus-on-load.module';


@NgModule({
  declarations: [SearchComponent],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,

    PipesModule,
    FocusOnLoadDirectiveModule,
    AutocompleteModule,
    SearchRoutingModule
  ]
})
export class SearchModule { }
