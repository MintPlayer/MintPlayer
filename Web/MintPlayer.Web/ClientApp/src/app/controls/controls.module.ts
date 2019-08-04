import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarTogglerComponent } from './navbar-toggler/navbar-toggler.component';



@NgModule({
  declarations: [NavbarTogglerComponent],
  imports: [
    CommonModule
  ],
  exports: [NavbarTogglerComponent]
})
export class ControlsModule { }
