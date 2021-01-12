import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxJsonLdModule } from '@ngx-lite/json-ld';
import { TranslateModule } from '@ngx-translate/core';

import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';
import { QueryParamsHandlingModule } from '../../directives/query-params-handling/query-params-handling.module';


@NgModule({
  declarations: [HomeComponent],
  imports: [
    CommonModule,
    NgxJsonLdModule,
    TranslateModule,
    QueryParamsHandlingModule,
    HomeRoutingModule,
  ]
})
export class HomeModule { }
