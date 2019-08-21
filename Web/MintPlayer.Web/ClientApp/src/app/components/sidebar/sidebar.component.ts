import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { SlideUpDownAnimation } from '../../../styles/animations/slide-up-down';
import { User } from '../../interfaces/account/user';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  animations: [
    SlideUpDownAnimation
  ]
})
export class SidebarComponent implements OnInit {

  constructor(private translateService: TranslateService) {
  }

  @Input() activeUser: User;

  level1menu: string = "";
  level2menu: string = "";
  level1toggle(menu: string) {
    if (this.level1menu === menu) {
      this.level1menu = "";
    } else {
      this.level1menu = menu;
    }
  }

  @Output() logoutClicked: EventEmitter<string> = new EventEmitter();
  onLogout() {
    this.logoutClicked.emit();
    this.itemSelected.emit();
  }

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();
  onItemSelected() {
    this.itemSelected.emit();
  }

  switchLanguage(language: string) {
    this.translateService.setDefaultLang(language);
  }

  ngOnInit() {
  }

}
