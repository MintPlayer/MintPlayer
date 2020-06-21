import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ForDirective } from './for/for.directive';
import { HoverClassDirective } from './hover-class/hover-class.directive';

@NgModule({
  declarations: [
    ForDirective,
    HoverClassDirective,
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ForDirective,
    HoverClassDirective,
  ]
})
export class DirectivesModule {
}
