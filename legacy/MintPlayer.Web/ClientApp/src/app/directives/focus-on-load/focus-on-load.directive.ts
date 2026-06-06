import { ViewContainerRef } from '@angular/core';
import { Directive, ElementRef, AfterViewInit, ComponentRef } from '@angular/core';

@Directive({
  selector: '*[autofocus]'
})
export class FocusOnLoadDirective implements AfterViewInit {

  constructor(
    private viewContainer: ViewContainerRef
  ) {
    const container = this.viewContainer['_lContainer'][0]
    if (container instanceof HTMLElement) {
      this.inputBox = <HTMLInputElement>container;
    } else {
      this.inputBox = container[8];
    }
  }

  private readonly inputBox!: any;

  ngAfterViewInit() {
    setTimeout(() => {
      this.inputBox.focus();
    }, 10);
  }
}
