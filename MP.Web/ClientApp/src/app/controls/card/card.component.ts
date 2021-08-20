import { Component, OnInit, Input, ElementRef, HostListener } from '@angular/core';
import { Point } from '../../entities/point';

@Component({
  selector: 'card',
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.scss']
})
export class CardComponent implements OnInit {

  elementOffset: Point = { x: 0, y: 0 };
  isMouseDown: boolean;
  private dragOffset: Point;

  constructor(private host: ElementRef) {
    this.isMouseDown = false;
    this.dragOffset = { x: 0, y: 0 };
  }

  @Input() public overflowX: boolean = true;
  @Input() public isDraggable: boolean = false;
  @Input() public noPadding: boolean = false;

  ngOnInit() {
  }

  //#region Dragging
  onDragStart(location: Point) {
    if (this.isDraggable) {
      this.isMouseDown = true;
      this.dragOffset = {
        x: location.x - this.elementOffset.x,
        y: location.y - this.elementOffset.y
      };
    }
  }
  onDragMove(location: Point, wrapper: { preventDefaultFn: () => void }) {
    if (this.isDraggable) {
      if (this.isMouseDown) {
        wrapper.preventDefaultFn();
        this.elementOffset = {
          x: location.x - this.dragOffset.x,
          y: location.y - this.dragOffset.y
        };
      }
    }
  }
  onDragEnd() {
    if (this.isDraggable) {
      this.isMouseDown = false;
    }
  }
  //#endregion

  //#region MouseEvents
  headerMouseDown($event: MouseEvent) {
    if (this.isDraggable) {
      $event.preventDefault();
    }
    this.onDragStart({ x: $event.clientX, y: $event.clientY });
  }

  @HostListener('document:mouseup', ['$event'])
  headerMouseUp($event: MouseEvent) {
    this.onDragEnd();
  }

  @HostListener('document:mousemove', ['$event'])
  headerMouseMove($event: MouseEvent) {
    this.onDragMove({ x: $event.clientX, y: $event.clientY }, {
      preventDefaultFn: () => {
        $event.preventDefault();
      }
    });
  }
  //#endregion

  //#region TouchEvents
  headerTouchStart($event: TouchEvent) {
    if ($event.touches.length === 1) {
      this.onDragStart({ x: $event.touches[0].clientX, y: $event.touches[0].clientY });
    }
  }
  headerTouchMove($event: TouchEvent) {
    this.onDragMove({ x: $event.touches[0].clientX, y: $event.touches[0].clientY }, {
      preventDefaultFn: () => {
        $event.preventDefault();
      }
    });
  }
  headerTouchEnd($event: TouchEvent) {
    this.onDragEnd();
  }
  //#endregion
}
