import { Component, OnInit } from '@angular/core';
import { SlideUpDownAnimation } from '../../../styles/animations/slide-up-down';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  animations: [
    SlideUpDownAnimation
  ]
})
export class SidebarComponent implements OnInit {

  constructor() {
  }

  level1menu: string = "";
  level2menu: string = "";
  level1toggle(menu: string) {
    if (this.level1menu === menu) {
      this.level1menu = "";
    } else {
      this.level1menu = menu;
    }
  }

  ngOnInit() {
  }

}
