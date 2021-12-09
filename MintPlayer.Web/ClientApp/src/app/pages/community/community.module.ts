import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CommunityRoutingModule } from './community-routing.module';
import { BlogModule } from './blog/blog.module';


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    CommunityRoutingModule,
    BlogModule
  ]
})
export class CommunityModule { }
