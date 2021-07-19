import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AutofocusDirective } from './autofocus.directive';

@NgModule({
  declarations: [
    AutofocusDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    AutofocusDirective
  ]
})
export class AutofocusDirectiveModule {
}
