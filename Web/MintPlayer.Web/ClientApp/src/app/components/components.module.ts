import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { SubjectModule } from './subject/subject.module';



@NgModule({
  declarations: [
    SidebarComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    SubjectModule
  ],
  exports: [
    SidebarComponent,
    SubjectModule
  ]
})
export class ComponentsModule { }
