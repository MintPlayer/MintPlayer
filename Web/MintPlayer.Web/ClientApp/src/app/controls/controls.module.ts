import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ClickOutsideModule } from 'ng-click-outside'; 
import { NavbarTogglerComponent } from './navbar-toggler/navbar-toggler.component';
import { CardComponent } from './card/card.component';
import { Select2Component } from './select2/select2.component';
import { PipesModule } from '../pipes/pipes.module';


@NgModule({
  declarations: [
    NavbarTogglerComponent,
    CardComponent,
    Select2Component
  ],
  imports: [
    CommonModule,
    FormsModule,
    PipesModule,
    ClickOutsideModule
  ],
  exports: [
    NavbarTogglerComponent,
    CardComponent,
    Select2Component
  ]
})
export class ControlsModule { }
