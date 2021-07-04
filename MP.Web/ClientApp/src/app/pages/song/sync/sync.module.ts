import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { SyncRoutingModule } from './sync-routing.module';
import { SyncComponent } from './sync.component';
import { CardModule } from '../../../controls/card/card.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';



@NgModule({
  declarations: [SyncComponent],
  imports: [
    CommonModule,
    FormsModule,

    CardModule,
    PipesModule,
    ForDirectiveModule,
    SyncRoutingModule
  ]
})
export class SyncModule { }
