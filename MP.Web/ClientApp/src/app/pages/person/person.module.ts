import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { PersonRoutingModule } from './person-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { ControlsModule } from '../../controls/controls.module';
import { ComponentsModule } from '../../components/components.module';
import { DirectivesModule } from '../../directives/directives.module';
import { PipesModule } from '../../pipes/pipes.module';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { FavoriteComponent } from './favorite/favorite.component';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    EditComponent,
    ShowComponent,
    FavoriteComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NgxJsonLdModule,
    ControlsModule,
    ComponentsModule,
    DirectivesModule,
    PipesModule,
    PersonRoutingModule
  ],
  providers: [
    SlugifyPipe
  ]
})
export class PersonModule { }
