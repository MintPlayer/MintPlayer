import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { ShowRoutingModule } from './show-routing.module';
import { ShowComponent } from './show.component';
import { PipesModule } from '../../../pipes/pipes.module';
import { SubjectLikeModule } from '../../../components/subject/subject-like/subject-like.module';
import { CardModule } from '../../../controls/card/card.module';
import { MediaListModule } from '../../../components/subject/media-list/media-list.module';



@NgModule({
  declarations: [ShowComponent],
  imports: [
    CommonModule,
    TranslateModule,
    AdvancedRouterModule,

    CardModule,
    PipesModule,
    MediaListModule,
    SubjectLikeModule,
    ShowRoutingModule
  ]
})
export class ShowModule { }
