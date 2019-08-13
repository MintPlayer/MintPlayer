import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacebookLoginComponent } from './facebook-login/facebook-login.component';
import { MicrosoftLoginComponent } from './microsoft-login/microsoft-login.component';
import { GoogleLoginComponent } from './google-login/google-login.component';



@NgModule({
  declarations: [FacebookLoginComponent, MicrosoftLoginComponent, GoogleLoginComponent],
  imports: [
    CommonModule
  ],
  exports: [FacebookLoginComponent, MicrosoftLoginComponent, GoogleLoginComponent]
})
export class SocialLoginsModule { }
