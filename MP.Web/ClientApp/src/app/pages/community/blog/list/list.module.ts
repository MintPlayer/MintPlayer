import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ListRoutingModule } from './list-routing.module';
import { ListComponent } from './list.component';
import { PipesModule } from '../../../../pipes/pipes.module';
import { DirectivesModule } from '../../../../directives/directives.module';


@NgModule({
  declarations: [ListComponent],
  imports: [
    CommonModule,
    PipesModule,
    AdvancedRouterModule,
    ListRoutingModule
  ]
})
export class ListModule { }
