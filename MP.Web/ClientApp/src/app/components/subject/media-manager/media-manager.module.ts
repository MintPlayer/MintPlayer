import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MediaManagerComponent } from './media-manager.component';

@NgModule({
  declarations: [
    MediaManagerComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DragDropModule,
 ],
  exports: [
    MediaManagerComponent
  ]
})
export class MediaManagerModule { }
