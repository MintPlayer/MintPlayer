import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacebookLoginComponent } from './facebook-login/facebook-login.component';
import { MicrosoftLoginComponent } from './microsoft-login/microsoft-login.component';



@NgModule({
  declarations: [FacebookLoginComponent, MicrosoftLoginComponent],
  imports: [
    CommonModule
  ],
  exports: [FacebookLoginComponent, MicrosoftLoginComponent]
})
export class SocialLoginsModule { }
