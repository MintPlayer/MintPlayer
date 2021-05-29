import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { FavoriteRoutingModule } from './favorite-routing.module';
import { FavoriteComponent } from './favorite.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [FavoriteComponent],
  imports: [
    CommonModule,
    AdvancedRouterModule,
    ControlsModule,
    PipesModule,

    FavoriteRoutingModule
  ]
})
export class FavoriteModule { }
