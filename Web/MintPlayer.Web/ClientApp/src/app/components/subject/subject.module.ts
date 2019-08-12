import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubjectLikeComponent } from './subject-like/subject-like.component';



@NgModule({
  declarations: [SubjectLikeComponent],
  imports: [
    CommonModule
  ],
  exports: [SubjectLikeComponent]
})
export class SubjectModule { }
