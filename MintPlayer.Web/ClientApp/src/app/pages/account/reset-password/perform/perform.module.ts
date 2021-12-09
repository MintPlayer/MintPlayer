import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { PerformRoutingModule } from './perform-routing.module';
import { PerformComponent } from './perform.component';
import { ForDirectiveModule } from '../../../../directives/for/for-directive.module';


@NgModule({
  declarations: [
    PerformComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ForDirectiveModule,
    AdvancedRouterModule,
    PerformRoutingModule
  ]
})
export class PerformModule { }
