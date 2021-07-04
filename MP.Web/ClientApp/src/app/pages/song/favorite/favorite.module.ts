import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { FavoriteRoutingModule } from './favorite-routing.module';
import { FavoriteComponent } from './favorite.component';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [FavoriteComponent],
  imports: [
    CommonModule,
    DatatablesModule,
    AdvancedRouterModule,

    PipesModule,
    FavoriteRoutingModule
  ]
})
export class FavoriteModule { }
