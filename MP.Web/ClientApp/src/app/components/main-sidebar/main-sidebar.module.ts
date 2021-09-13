import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ShareButtonsModule } from '@mintplayer/ng-share-buttons';
import { AdvancedRouterModule } from '@mintplayer/ng-router';
import { SidebarComponent } from './main-sidebar.component';
import { FlagModule } from '../flag/flag.module';
import { HoverClassDirectiveModule } from '../../directives/hover-class/hover-class-directive.module';

@NgModule({
  declarations: [
    SidebarComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
    ShareButtonsModule,
    AdvancedRouterModule,
    FlagModule,
    HoverClassDirectiveModule
  ],
  exports: [
    SidebarComponent
  ]
})
export class SidebarModule { }
