import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { SubjectModule } from './subject/subject.module';
import { SocialLoginsModule } from './social-logins/social-logins.module';
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
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent,
    DailymotionPlayerComponent,
    FlagComponent
  ],
  imports: [
    CommonModule,
    AdvancedRouterModule,
    PipesModule,
    DirectivesModule,
    ControlsModule,
    SubjectModule,
    TranslateModule,
    SocialLoginsModule,
    SocialSharesModule
  ],
  exports: [
    SidebarComponent,
    SubjectModule,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent,
    DailymotionPlayerComponent,
    FlagComponent
  ]
})
export class ComponentsModule { }
