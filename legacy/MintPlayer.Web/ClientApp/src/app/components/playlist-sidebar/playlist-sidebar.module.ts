import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';
import { PlaylistSidebarComponent } from './playlist-sidebar.component';
import { ProgressBarModule } from '../../controls/progress-bar/progress-bar.module';
import { PipesModule } from '../../pipes/pipes.module';

@NgModule({
  declarations: [
    PlaylistSidebarComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,
    ProgressBarModule,
    PipesModule
  ],
  exports: [
    PlaylistSidebarComponent
  ]
})
export class PlaylistSidebarModule { }
