import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { SubjectModule } from './subject/subject.module';
import { SocialLoginsModule } from './social-logins/social-logins.module';



@NgModule({
  declarations: [
    SidebarComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    SubjectModule,
    SocialLoginsModule
  ],
  exports: [
    SidebarComponent,
    SubjectModule
  ]
})
export class ComponentsModule { }
