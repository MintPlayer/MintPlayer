import { Component, OnInit, Input, HostListener, TemplateRef, ViewChild, ViewChildren, ElementRef, QueryList } from '@angular/core';
import { Point } from '../../entities/point';

@Component({
  selector: 'app-sortable-list',
  templateUrl: './sortable-list.component.html',
  styleUrls: ['./sortable-list.component.scss']
})
export class SortableListComponent implements OnInit {

  constructor() {
  }

  private isBusy: boolean = false;
  private referencePoint: Point = { x: 0, y: 0 };
  captureDiff: Point = { x: 0, y: 0 };
  @Input() items: any[];
  capturedItem: any = null;

  @ViewChildren('li') lis: QueryList<ElementRef<HTMLUListElement>>;

  //#region hoveredItem
  hoveredItem: any = null;
  itemMouseEnter($event: MouseEvent, item: any) {
    this.hoveredItem = item;
  }
  itemMouseLeave($event: MouseEvent, item: any) {
    this.hoveredItem = null;
  }
  //#endregion

  //#region ItemTemplate
  @Input() itemTemplate: TemplateRef<any>;
  //#endregion

  //#region Dragging
  onDragStart(item: any, location: Point) {
    this.capturedItem = item;
    this.referencePoint = location;
    this.captureDiff = { x: 0, y: 0 };
  }
  onDragMove(location: Point, wrapper: { preventDefaultFn: () => void }) {
    if ((this.isBusy === false) && (this.capturedItem !== null)) {
      wrapper.preventDefaultFn();

      let diff: Point = {
        x: location.x - this.referencePoint.x,
        y: location.y - this.referencePoint.y
      };

      let index = this.items.indexOf(this.capturedItem);

      let prevHeight = index >= 1 ? this.lis.toArray()[index - 1].nativeElement.clientHeight : 49;
      let nextHeight = index < this.lis.length - 1 ? this.lis.toArray()[index + 1].nativeElement.clientHeight : 49;

      if (diff.y >= nextHeight) {
        this.isBusy = true;

        // Move down
        if (index < this.items.length - 1) {
          let next = this.items[index + 1];

          this.items[index] = next;
          this.items[index + 1] = this.capturedItem;

          this.referencePoint.y += nextHeight;
          this.captureDiff = {
            x: diff.x,
            y: diff.y - nextHeight
          };
        } else {
          this.captureDiff = diff;
        }
        this.isBusy = false;
      } else if (diff.y <= -prevHeight) {
        this.isBusy = true;

        // Move up
        if (index > 0) {
          let prev = this.items[index - 1];

          this.items[index] = prev;
          this.items[index - 1] = this.capturedItem;

          this.referencePoint.y -= prevHeight;
          this.captureDiff = {
            x: diff.x,
            y: diff.y + prevHeight
          };
        } else {
          this.captureDiff = diff;
        }
        this.isBusy = false;
      } else {
        this.captureDiff = diff;
      }
    }
  }
  onDragEnd() {
    this.capturedItem = null;
  }
  //#endregion

  //#region MouseEvents
  itemMouseDown($event: MouseEvent, li: HTMLUListElement, item: any) {
    $event.preventDefault();
    if ($event.target === li) {
      this.onDragStart(item, { x: $event.clientX, y: $event.clientY });
    }
    console.log(this.lis);
  }

  @HostListener('document:mousemove', ['$event'])
  mouseMove($event: MouseEvent) {
    $event.stopPropagation();
    this.onDragMove({ x: $event.clientX, y: $event.clientY }, {
      preventDefaultFn: () => {
        $event.preventDefault();
      }
    });
  }

  @HostListener('document:mouseup', ['$event'])
  mouseUp($event: MouseEvent) {
    this.onDragEnd();
  }

  trackByFn = (item) => {
    return item;
  };
  //#endregion
  //#region TouchEvents
  itemTouchStart($event: TouchEvent, li: HTMLUListElement, item: any) {
    if ($event.target === li) {
      if ($event.touches.length === 1) {
        this.onDragStart(item, { x: $event.touches[0].clientX, y: $event.touches[0].clientY });
      }
    }
  }
  itemTouchMove($event: TouchEvent, item: any) {
    this.onDragMove({ x: $event.touches[0].clientX, y: $event.touches[0].clientY }, {
      preventDefaultFn: () => {
        $event.preventDefault();
      }
    });
  }
  itemTouchEnd($event: TouchEvent, item: any) {
    this.onDragEnd();
  }
  //#endregion

  ngOnInit() {
  }

}
