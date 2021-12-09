import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { YoutubePlayButtonComponent } from './youtube-play-button.component';

@NgModule({
  declarations: [
    YoutubePlayButtonComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
  ],
  exports: [
    YoutubePlayButtonComponent
  ]
})
export class YoutubePlayButtonModule { }
