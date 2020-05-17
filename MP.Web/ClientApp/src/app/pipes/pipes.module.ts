import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InListPipe } from './in-list/in-list.pipe';
import { KeepHtmlPipe } from './keep-html/keep-html.pipe';
import { FontColorPipe } from './font-color/font-color.pipe';
import { SlugifyPipe } from './slugify/slugify.pipe';
import { PropValPipe } from './prop-val/prop-val.pipe';
import { LinifyPipe } from './linify/linify.pipe';
import { WordCountPipe } from './word-count/word-count.pipe';

@NgModule({
  declarations: [
    InListPipe,
    KeepHtmlPipe,
    FontColorPipe,
    SlugifyPipe,
    PropValPipe,
    LinifyPipe,
    WordCountPipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    InListPipe,
    KeepHtmlPipe,
    FontColorPipe,
    SlugifyPipe,
    PropValPipe,
    LinifyPipe,
    WordCountPipe
  ]
})
export class PipesModule { }
