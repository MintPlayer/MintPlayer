import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { BlogRoutingModule } from './blog-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { PipesModule } from '../../../pipes/pipes.module';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ControlsModule } from '../../../controls/controls.module';
import { DirectivesModule } from '../../../directives/directives.module';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';
import { WordCountPipe } from '../../../pipes/word-count/word-count.pipe';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    EditComponent,
    ShowComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    PipesModule,
    DirectivesModule,
    RouterModule,
    NgxJsonLdModule,
    BlogRoutingModule
  ],
  providers: [
    SlugifyPipe,
    WordCountPipe
  ]
})
export class BlogModule { }
