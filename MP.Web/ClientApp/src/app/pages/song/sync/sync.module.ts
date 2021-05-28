import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { SyncRoutingModule } from './sync-routing.module';
import { SyncComponent } from './sync.component';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { PipesModule } from '../../../pipes/pipes.module';



@NgModule({
  declarations: [SyncComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    DirectivesModule,

    PipesModule,
    SyncRoutingModule
  ]
})
export class SyncModule { }
