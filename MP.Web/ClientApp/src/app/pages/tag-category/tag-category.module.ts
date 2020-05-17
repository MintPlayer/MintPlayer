import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TagModule } from './tag/tag.module';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ListComponent } from './list/list.component';
import { ShowComponent } from './show/show.component';
import { TagCategoryRoutingModule } from './tag-category-routing.module';
import { ControlsModule } from '../../controls/controls.module';
import { PipesModule } from '../../pipes/pipes.module';
import { DirectivesModule } from '../../directives/directives.module';


@NgModule({
  declarations: [
    CreateComponent,
    EditComponent,
    ListComponent,
    ShowComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ControlsModule,
    DirectivesModule,
    PipesModule,
    TagCategoryRoutingModule,
  ]
})
export class TagCategoryModule { }
