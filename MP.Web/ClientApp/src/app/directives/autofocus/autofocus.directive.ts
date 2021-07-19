import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
  selector: '[autofocus]'
})
export class AutofocusDirective implements AfterViewInit {

  constructor(private element: ElementRef) {
  }

  ngAfterViewInit() {
    this.element.nativeElement.focus();
  }
}
