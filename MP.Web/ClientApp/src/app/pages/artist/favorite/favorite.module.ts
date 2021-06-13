import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { FavoriteRoutingModule } from './favorite-routing.module';
import { FavoriteComponent } from './favorite.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [FavoriteComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,
    DatatablesModule,
    AdvancedRouterModule,
    FavoriteRoutingModule
  ]
})
export class FavoriteModule { }
