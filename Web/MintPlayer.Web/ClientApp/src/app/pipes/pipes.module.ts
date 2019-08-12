import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InListPipe } from './in-list/in-list.pipe';



@NgModule({
  declarations: [InListPipe],
  imports: [
    CommonModule
  ],
  exports: [InListPipe]
})
export class PipesModule { }
