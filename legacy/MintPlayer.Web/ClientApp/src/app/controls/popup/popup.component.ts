import { Component, OnInit, Input } from '@angular/core';
import { FadeInOutAnimation } from '../../../styles/animations/fade-in-out.animation';

@Component({
  selector: 'app-popup',
  templateUrl: './popup.component.html',
  styleUrls: ['./popup.component.scss'],
  animations: [
    FadeInOutAnimation
  ]
})
export class PopupComponent implements OnInit {

  constructor() {
  }

  ngOnInit() {
  }

  @Input() dialogVisible: boolean;

}
