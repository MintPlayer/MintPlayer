import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ForDirective } from './for/for.directive';
import { HoverClassDirective } from './hover-class/hover-class.directive';
import { QueryParamsHandlingDirective } from './query-params-handling/query-params-handling.directive';

@NgModule({
  declarations: [
    ForDirective,
    HoverClassDirective,
    QueryParamsHandlingDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ForDirective,
    HoverClassDirective,
    QueryParamsHandlingDirective
  ]
})
export class DirectivesModule { }
