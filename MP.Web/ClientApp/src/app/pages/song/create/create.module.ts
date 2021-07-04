import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Select2Module } from '@mintplayer/ng-select2';

import { CreateRoutingModule } from './create-routing.module';
import { CreateComponent } from './create.component';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { MediaManagerModule } from '../../../components/subject/media-manager/media-manager.module';



@NgModule({
  declarations: [CreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    Select2Module,

    CardModule,
    PipesModule,
    ForDirectiveModule,
    MediaManagerModule,

    CreateRoutingModule
  ]
})
export class CreateModule { }
