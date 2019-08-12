import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarTogglerComponent } from './navbar-toggler/navbar-toggler.component';
import { CardComponent } from './card/card.component';



@NgModule({
  declarations: [NavbarTogglerComponent, CardComponent],
  imports: [
    CommonModule
  ],
  exports: [NavbarTogglerComponent, CardComponent]
})
export class ControlsModule { }
