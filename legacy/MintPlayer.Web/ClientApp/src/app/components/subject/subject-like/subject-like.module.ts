import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SubjectLikeComponent } from './subject-like.component';

@NgModule({
  declarations: [
    SubjectLikeComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
  ],
  exports: [
    SubjectLikeComponent
  ]
})
export class SubjectLikeModule { }
