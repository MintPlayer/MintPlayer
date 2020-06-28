import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FetchRoutingModule } from './fetch-routing.module';
import { FetchComponent } from './fetch.component';


@NgModule({
  declarations: [FetchComponent],
  imports: [
    CommonModule,
    FetchRoutingModule
  ]
})
export class FetchModule { }
