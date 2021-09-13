import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FocusOnLoadDirective } from './focus-on-load.directive';

@NgModule({
  declarations: [
    FocusOnLoadDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    FocusOnLoadDirective
  ]
})
export class FocusOnLoadDirectiveModule {
}
