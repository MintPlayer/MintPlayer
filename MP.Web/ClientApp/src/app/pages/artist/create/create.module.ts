import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { ComponentsModule } from '../../../components/components.module';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { PipesModule } from '../../../pipes/pipes.module';


@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ControlsModule,
    ComponentsModule,
    DirectivesModule,
    PipesModule,
    CreateRoutingModule
  ]
})
export class CreateModule { }
