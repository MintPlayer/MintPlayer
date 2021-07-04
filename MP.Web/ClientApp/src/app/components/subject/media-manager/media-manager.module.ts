import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MediaManagerComponent } from './media-manager.component';

@NgModule({
  declarations: [
    MediaManagerComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
  ],
  exports: [
    MediaManagerComponent
  ]
})
export class MediaManagerModule { }
