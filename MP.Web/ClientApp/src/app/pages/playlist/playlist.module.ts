import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { PlaylistRoutingModule } from './playlist-routing.module';
import { PlaylistCreateComponent } from './create/create.component';
import { PlaylistEditComponent } from './edit/edit.component';
import { PlaylistListComponent } from './list/list.component';
import { PlaylistShowComponent } from './show/show.component';
import { ControlsModule } from '../../controls/controls.module';
import { PipesModule } from '../../pipes/pipes.module';
import { DirectivesModule } from '../../directives/directives.module';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';


@NgModule({
  declarations: [
    PlaylistCreateComponent,
    PlaylistEditComponent,
    PlaylistListComponent,
    PlaylistShowComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    PipesModule,
    ControlsModule,
    DirectivesModule,
    PlaylistRoutingModule
  ],
  providers: [
    SlugifyPipe
  ]
})
export class PlaylistModule { }
