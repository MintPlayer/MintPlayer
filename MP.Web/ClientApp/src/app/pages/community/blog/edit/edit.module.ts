import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { EditRoutingModule } from './edit-routing.module';
import { EditComponent } from './edit.component';
import { ControlsModule } from '../../../../controls/controls.module';
import { DirectivesModule } from '../../../../directives/directives.module';


@NgModule({
  declarations: [EditComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ControlsModule,
    DirectivesModule,
    EditRoutingModule
  ]
})
export class EditModule { }
