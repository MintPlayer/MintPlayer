import { Component, OnInit, Output, EventEmitter, Input, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { User } from '../../entities/user';
import { SlideUpDownAnimation } from '../../../styles/animations/slide-up-down.animation';

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

  level1menu: string = '';
  level2menu: string = '';
  level1toggle(menu: string) {
    if (this.level1menu === menu) {
      this.level1menu = '';
    } else {
      this.level1menu = menu;
    }
  }
  level2toggle(menu: string) {
    if (this.level2menu === menu) {
      this.level2menu = '';
    } else {
      this.level2menu = menu;
    }
  }

  @Input() activeUser: User;
  @Output() logoutClicked: EventEmitter<string> = new EventEmitter();
  onLogout() {
    this.logoutClicked.emit();
    this.itemSelected.emit();
  }

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();
  onItemSelected() {
    this.itemSelected.emit();
  }

  ngOnInit() {
  }
}
