import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
  selector: 'input[autofocus]'
})
export class FocusOnLoadDirective implements AfterViewInit {

  constructor(private element: ElementRef) {
  }

  ngAfterViewInit() {
    this.element.nativeElement.focus();
  }
}
