import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { SubjectModule } from './subject/subject.module';
import { SocialLoginsModule } from './social-logins/social-logins.module';
import { YoutubePlayerComponent } from './youtube-player/youtube-player.component';
import { YoutubePlayButtonComponent } from './youtube-play-button/youtube-play-button.component';
import { PlaylistSidebarComponent } from './playlist-sidebar/playlist-sidebar.component';
import { ControlsModule } from '../controls/controls.module';
import { PipesModule } from '../pipes/pipes.module';
import { DailymotionPlayerComponent } from './dailymotion-player/dailymotion-player.component';
import { SocialSharesModule } from './social-shares/social-shares.module';

@NgModule({
  declarations: [
    SidebarComponent,
    YoutubePlayerComponent,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent,
    DailymotionPlayerComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    PipesModule,
    ControlsModule,
    SubjectModule,
    SocialLoginsModule,
    SocialSharesModule
  ],
  exports: [
    SidebarComponent,
    SubjectModule,
    YoutubePlayerComponent,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent,
    DailymotionPlayerComponent
  ]
})
export class ComponentsModule { }
