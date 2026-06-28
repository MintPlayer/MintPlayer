import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { FavoriteRoutingModule } from './favorite-routing.module';
import { FavoriteComponent } from './favorite.component';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [FavoriteComponent],
  imports: [
    CommonModule,
    TranslateModule,
    DatatablesModule,
    AdvancedRouterModule,

    PipesModule,
    FavoriteRoutingModule
  ]
})
export class FavoriteModule { }
