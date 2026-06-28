import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrls: ['./progress-bar.component.scss']
})
export class ProgressBarComponent implements OnInit {

  constructor() {
  }

  //#region Value
  private _value: number;
  get value() {
    return this._value;
  }
  @Input() set value(value: number) {
    this._value = value;
    this.updatePercentage();
  }
  //#endregion
  //#region Minimum
  private _minimum: number;
  get minimum() {
    return this._minimum;
  }
  @Input() set minimum(value: number) {
    this._minimum = value;
    this.updatePercentage();
  }
  //#endregion
  //#region Maximum
  private _maximum: number;
  get maximum() {
    return this._maximum;
  }
  @Input() set maximum(value: number) {
    this._maximum = value;
    this.updatePercentage();
  }
  //#endregion
  //#region Percentage
  percentage: string = '0%';
  private updatePercentage() {
    let val = (this._value - this._minimum) / (this._maximum - this._minimum);
    this.percentage = (val * 100) + '%';
  }
  //#endregion

  //#region Flat
  private _flat: boolean = false;
  get flat() {
    return this._flat;
  }
  @Input() set flat(value: boolean) {
    this._flat = value;
  }
  //#endregion

  ngOnInit() {
  }

}
