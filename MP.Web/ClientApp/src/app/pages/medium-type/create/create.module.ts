import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { DirectivesModule } from '../../../directives/directives.module';



@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DirectivesModule,

    CreateRoutingModule
  ]
})
export class CreateModule { }
