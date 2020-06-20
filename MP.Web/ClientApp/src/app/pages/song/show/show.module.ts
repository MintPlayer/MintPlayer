import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { ControlsModule } from '../../../controls/controls.module';
import { ComponentsModule } from '../../../components/components.module';
import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    NgxJsonLdModule,
    ControlsModule,
    ComponentsModule,
    PipesModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }