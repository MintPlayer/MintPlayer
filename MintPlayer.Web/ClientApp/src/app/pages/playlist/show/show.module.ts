import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ShowRoutingModule } from './show-routing.module';
import { PlaylistShowComponent } from './show.component';
import { PipesModule } from '../../../pipes/pipes.module';
import { CardModule } from '../../../controls/card/card.module';



@NgModule({
  declarations: [PlaylistShowComponent],
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
