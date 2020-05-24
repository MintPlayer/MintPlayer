import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';
import { TranslateModule } from '@ngx-translate/core';

import { PagesRoutingModule } from './pages-routing.module';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { PipesModule } from '../pipes/pipes.module';
import { ControlsModule } from '../controls/controls.module';
import { CommunityModule } from './community/community.module';
import { GdprModule } from './gdpr/gdpr.module';


@NgModule({
  declarations: [
    HomeComponent,
    SearchComponent,
    NotFoundComponent
  ],
  imports: [
    CommonModule,
    ControlsModule,
    PipesModule,
    NgxJsonLdModule,
    TranslateModule,
    PagesRoutingModule,
    CommunityModule,
    GdprModule
  ]
})
export class PagesModule { }
