import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
//import { JsonLdModule } from '@mintplayer/ng-json-ld';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../../pipes/pipes.module';
import { DirectivesModule } from '../../../../directives/directives.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    //JsonLdModule,
    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
