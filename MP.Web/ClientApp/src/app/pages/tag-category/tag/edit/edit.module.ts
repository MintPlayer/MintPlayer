import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { ControlsModule } from '../../../../controls/controls.module';
import { DirectivesModule } from '../../../../directives/directives.module';
import { QueryParamsHandlingModule } from '../../../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    DirectivesModule,
    QueryParamsHandlingModule,
    EditRoutingModule
  ]
})
export class EditModule { }
