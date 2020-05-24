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
import { FlagComponent } from './flag/flag.component';
import { DirectivesModule } from '../directives/directives.module';

@NgModule({
  declarations: [
    SidebarComponent,
    YoutubePlayerComponent,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent,
    DailymotionPlayerComponent,
    FlagComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    PipesModule,
    DirectivesModule,
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
    DailymotionPlayerComponent,
    FlagComponent
  ]
})
export class ComponentsModule { }
