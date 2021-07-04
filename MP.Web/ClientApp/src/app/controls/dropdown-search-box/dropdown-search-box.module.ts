import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { DropdownSearchBoxComponent } from './dropdown-search-box.component';
import { ProgressBarModule } from '../progress-bar/progress-bar.module';
import { PipesModule } from '../../pipes/pipes.module';

@NgModule({
  declarations: [
    DropdownSearchBoxComponent
  ],
  imports: [
    CommonModule,
    FormsModule,

    PipesModule,
    ProgressBarModule
  ],
  exports: [
    DropdownSearchBoxComponent
  ]
})
export class DropdownSearchBoxModule { }
