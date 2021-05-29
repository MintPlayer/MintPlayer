import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { NotFoundRoutingModule } from './not-found-routing.module';
import { NotFoundComponent } from './not-found.component';
import { DirectivesModule } from '../../directives/directives.module';


@NgModule({
  declarations: [NotFoundComponent],
  imports: [
    CommonModule,
    AdvancedRouterModule,
    NotFoundRoutingModule
  ]
})
export class NotFoundModule { }
