import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacebookLoginComponent } from './facebook-login/facebook-login.component';
import { MicrosoftLoginComponent } from './microsoft-login/microsoft-login.component';
import { GoogleLoginComponent } from './google-login/google-login.component';
import { TwitterLoginComponent } from './twitter-login/twitter-login.component';



@NgModule({
  declarations: [FacebookLoginComponent, MicrosoftLoginComponent, GoogleLoginComponent, TwitterLoginComponent],
  imports: [
    CommonModule
  ],
  exports: [FacebookLoginComponent, MicrosoftLoginComponent, GoogleLoginComponent, TwitterLoginComponent]
})
export class SocialLoginsModule { }
