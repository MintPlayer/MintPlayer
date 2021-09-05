import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DatatablesModule } from '@mintplayer/ng-datatables';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ListRoutingModule } from './list-routing.module';
import { PlaylistListComponent } from './list.component';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [PlaylistListComponent],
  imports: [
    CommonModule,
    DatatablesModule,
    TranslateModule,
    AdvancedRouterModule,

    PipesModule,
    ListRoutingModule
  ]
})
export class ListModule { }
