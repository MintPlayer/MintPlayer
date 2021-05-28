import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { ShowRoutingModule } from './show-routing.module';
import { PlaylistShowComponent } from './show.component';
import { ControlsModule } from '../../../controls/controls.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [PlaylistShowComponent],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,

    NgxJsonLdModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
