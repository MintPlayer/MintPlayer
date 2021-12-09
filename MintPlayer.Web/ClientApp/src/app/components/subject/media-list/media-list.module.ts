import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MediaListComponent } from './media-list.component';

@NgModule({
  declarations: [
    MediaListComponent
  ],
  imports: [
    CommonModule,
    TranslateModule
  ],
  exports: [
    MediaListComponent
  ]
})
export class MediaListModule { }
