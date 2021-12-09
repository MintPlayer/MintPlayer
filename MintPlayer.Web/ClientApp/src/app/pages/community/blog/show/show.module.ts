import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { JsonLdModule } from '@mintplayer/ng-json-ld';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../../pipes/pipes.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    JsonLdModule,
    TranslateModule,
    AdvancedRouterModule,

    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
