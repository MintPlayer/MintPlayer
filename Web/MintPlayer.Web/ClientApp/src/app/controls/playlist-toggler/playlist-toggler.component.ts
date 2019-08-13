import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { eToggleButtonState } from '../../enums/eToggleButtonState';

@Component({
  selector: 'playlist-toggler',
  templateUrl: './playlist-toggler.component.html',
  styleUrls: ['./playlist-toggler.component.scss']
})
export class PlaylistTogglerComponent implements OnInit {

  constructor() {
  }

  ngOnInit() {
  }

  @Input() state: eToggleButtonState;
  @Output() stateChange: EventEmitter<eToggleButtonState> = new EventEmitter();

  toggle() {
    switch (this.state) {
      case eToggleButtonState.open:
        this.state = eToggleButtonState.closed;
        break;
      default:
        this.state = eToggleButtonState.open;
        break;
    }
    this.stateChange.emit(this.state);
  }

}
