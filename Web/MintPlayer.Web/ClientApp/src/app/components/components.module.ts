import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SidebarComponent } from './sidebar/sidebar.component';
import { SubjectModule } from './subject/subject.module';
import { SocialLoginsModule } from './social-logins/social-logins.module';
import { YoutubePlayerComponent } from './youtube-player/youtube-player.component';
import { YoutubePlayButtonComponent } from './youtube-play-button/youtube-play-button.component';
import { PlaylistSidebarComponent } from './playlist-sidebar/playlist-sidebar.component';



@NgModule({
  declarations: [
    SidebarComponent,
    YoutubePlayerComponent,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    SubjectModule,
    SocialLoginsModule,
    TranslateModule.forChild()
  ],
  exports: [
    SidebarComponent,
    SubjectModule,
    YoutubePlayerComponent,
    YoutubePlayButtonComponent,
    PlaylistSidebarComponent
  ]
})
export class ComponentsModule { }
