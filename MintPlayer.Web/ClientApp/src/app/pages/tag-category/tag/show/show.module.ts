import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { CardModule } from '../../../../controls/card/card.module';
import { PipesModule } from '../../../../pipes/pipes.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,

    CardModule,
    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
