import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PaginationResponse } from '../../helpers/pagination-response';
import { PaginationRequest } from '../../helpers/pagination-request';
//import { get } from 'lodash';

@Component({
  selector: 'pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent implements OnInit {

  constructor() {
  }

  //#region Numbers
  private _numbers: number[];
  @Input() set numbers(value: number[]) {
    this._numbers = value;
  }
  get numbers() {
    return this._numbers;
  }
  //#endregion

  //#region SelectedNumber
  @Input() selectedNumber: number;
  @Output() selectedNumberChange: EventEmitter<number> = new EventEmitter();
  onSelectedNumberChange(event: MouseEvent, number: number) {
    event.preventDefault();
    this.selectedNumberChange.emit(number);
  }
  //#endregion

  //#region ShowArrows
  @Input() showArrows: boolean;
  //#endregion
  
  //#region Event handlers
  onPrevious(event: MouseEvent) {
    event.preventDefault();
    let current_index = this.numbers.indexOf(this.selectedNumber);
    switch (current_index) {
      case -1:
        this.onSelectedNumberChange(event, this.numbers[0]);
        break;
      case 0:
        break;
      default:
        this.onSelectedNumberChange(event, this.numbers[current_index - 1]);
        break;
    }
  }
  onNext(event: MouseEvent) {
    event.preventDefault();
    let current_index = this.numbers.indexOf(this.selectedNumber);
    switch (current_index) {
      case -1:
        this.onSelectedNumberChange(event, this.numbers[this.numbers.length - 1]);
        break;
      case this.numbers.length - 1:
        break;
      default:
        this.onSelectedNumberChange(event, this.numbers[current_index + 1]);
        break;
    }
  }
  //#endregion

  ////#region Navigated
  //@Output() navigated: EventEmitter<PaginationRequest> = new EventEmitter();
  //onNavigated(event: MouseEvent, number: number) {
  //  event.preventDefault();
  //  this.navigated.emit({
  //    perPage: this._state.perPage,
  //    page: number
  //  });
  //}
  ////#endregion

  ngOnInit() {
  }

}
