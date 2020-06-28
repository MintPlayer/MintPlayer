import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { FetchRoutingModule } from './fetch-routing.module';
import { FetchComponent } from './fetch.component';
import { PipesModule } from '../../pipes/pipes.module';


@NgModule({
  declarations: [FetchComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    PipesModule,
    FetchRoutingModule
  ]
})
export class FetchModule { }
