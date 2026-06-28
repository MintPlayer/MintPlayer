import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { eToggleButtonState } from '../../enums/eToggleButtonState';

@Component({
  selector: 'app-navbar-toggler',
  templateUrl: './navbar-toggler.component.html',
  styleUrls: ['./navbar-toggler.component.scss']
})
export class NavbarTogglerComponent implements OnInit {

  @Input() state: eToggleButtonState;
  @Output() stateChange: EventEmitter<eToggleButtonState> = new EventEmitter();

  constructor() {
  }

  ngOnInit() {
  }

  toggle() {
    switch (this.state) {
      case eToggleButtonState.open:
        this.state = eToggleButtonState.closed;
        break;
      case eToggleButtonState.closed:
        this.state = eToggleButtonState.open;
        break;
      default:
        if (window.innerWidth > 767) {
          this.state = eToggleButtonState.closed;
        } else {
          this.state = eToggleButtonState.open;
        }
        break;
    }
    this.stateChange.emit(this.state);
  }
}
