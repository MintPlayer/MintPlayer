import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubjectLikeComponent } from './subject-like/subject-like.component';
import { MediaManagerComponent } from './media-manager/media-manager.component';



@NgModule({
  declarations: [SubjectLikeComponent, MediaManagerComponent],
  imports: [
    CommonModule
  ],
  exports: [SubjectLikeComponent, MediaManagerComponent]
})
export class SubjectModule { }
