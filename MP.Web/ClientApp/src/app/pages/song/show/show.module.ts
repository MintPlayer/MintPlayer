import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JsonLdModule } from '@mintplayer/ng-json-ld';
import { AdvancedRouterModule } from '@mintplayer/ng-router';
import { ShareButtonsModule } from '@mintplayer/ng-share-buttons';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../pipes/pipes.module';
import { SubjectLikeModule } from '../../../components/subject/subject-like/subject-like.module';
import { YoutubePlayButtonModule } from '../../../components/youtube-play-button/youtube-play-button.module';
import { CardModule } from '../../../controls/card/card.module';
import { MediaListModule } from '../../../components/subject/media-list/media-list.module';



@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    JsonLdModule,
    AdvancedRouterModule,

    CardModule,
    PipesModule,
    MediaListModule,
    SubjectLikeModule,
    ShareButtonsModule,
    YoutubePlayButtonModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
