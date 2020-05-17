import { Directive, ElementRef, Input, HostListener } from '@angular/core';

@Directive({
  selector: '[appFor]'
})
export class ForDirective {

  constructor() {
  }

  @Input() appFor: any;
  @HostListener('click') onMouseClick() {
    this.appFor.focus();
  }
}
