import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ColorTransitionAnimation } from '../../../styles/animations/color-transition.animation';

@Component({
  selector: 'toggle-button',
  templateUrl: './toggle-button.component.html',
  styleUrls: ['./toggle-button.component.scss'],
  animations: [
    ColorTransitionAnimation
  ]
})
export class ToggleButtonComponent implements OnInit {

  constructor() {
  }

  ngOnInit() {
  }

  //#region isToggled
  _isToggled: boolean = false;
  @Output() public isToggledChange = new EventEmitter<boolean>();
  public get isToggled() {
    return this._isToggled;
  }
  @Input() public set isToggled(value: boolean) {
    console.log(this._isToggled);
    this._isToggled = value;
    this.isToggledChange.emit(this._isToggled);
  }
  //#endregion

  //#region offColor
  @Input() public offColor: string = '#CCCCCC';
  //#endregion
  //#region onColor
  @Input() public onColor: string = '#2196F3';
  //#endregion
  //#region round
  @Input() public round: boolean = true;
  //#endregion
}
