import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BlogRoutingModule } from './blog-routing.module';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';
import { WordCountPipe } from '../../../pipes/word-count/word-count.pipe';


@NgModule({
  declarations: [
  ],
  imports: [
    CommonModule,
    BlogRoutingModule
  ],
  providers: [
    SlugifyPipe,
    WordCountPipe
  ]
})
export class BlogModule { }
