import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { ComponentsModule } from '../../../components/components.module';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { QueryParamsHandlingModule } from '../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    ComponentsModule,
    DirectivesModule,
    QueryParamsHandlingModule,
    PipesModule,
    EditRoutingModule
  ]
})
export class EditModule { }
