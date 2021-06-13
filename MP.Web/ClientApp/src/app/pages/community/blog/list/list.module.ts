import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { PipesModule } from '../../../../pipes/pipes.module';


@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    TranslateModule,
    PipesModule,
    AdvancedRouterModule,
    ListRoutingModule
  ]
})
export class ListModule { }
