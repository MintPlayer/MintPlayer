import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AutocompleteComponent } from './autocomplete.component';



@NgModule({
  declarations: [
    AutocompleteComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
  ],
  exports: [
    AutocompleteComponent
  ]
})
export class AutocompleteModule { }
