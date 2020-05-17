import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ForDirective } from './for/for.directive';

@NgModule({
  declarations: [
    ForDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ForDirective
  ]
})
export class DirectivesModule { }
