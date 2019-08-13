import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacebookLoginComponent } from './facebook-login/facebook-login.component';



@NgModule({
  declarations: [FacebookLoginComponent],
  imports: [
    CommonModule
  ],
  exports: [FacebookLoginComponent]
})
export class SocialLoginsModule { }
