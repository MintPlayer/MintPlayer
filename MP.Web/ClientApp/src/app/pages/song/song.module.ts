import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';

import { SongRoutingModule } from './song-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { ControlsModule } from '../../controls/controls.module';
import { ComponentsModule } from '../../components/components.module';
import { PipesModule } from '../../pipes/pipes.module';
import { DirectivesModule } from '../../directives/directives.module';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { SyncComponent } from './sync/sync.component';
import { SocialSharesModule } from '../../components/social-shares/social-shares.module';
import { FavoriteComponent } from './favorite/favorite.component';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    EditComponent,
    ShowComponent,
    SyncComponent,
    FavoriteComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NgxJsonLdModule,
    ControlsModule,
    SocialSharesModule,
    DirectivesModule,
    PipesModule,
    ComponentsModule,
    SongRoutingModule
  ],
  providers: [
    SlugifyPipe
  ]
})
export class SongModule { }
