import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { PublicRoutingModule } from './public-routing.module';
import { PlaylistPublicComponent } from './public.component';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [PlaylistPublicComponent],
  imports: [
    CommonModule,
    TranslateModule,
    DatatablesModule,
    AdvancedRouterModule,

    PipesModule,
    PublicRoutingModule
  ]
})
export class PublicModule { }
