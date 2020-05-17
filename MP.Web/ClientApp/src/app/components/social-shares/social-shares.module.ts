import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FacebookShareComponent } from './facebook-share/facebook-share.component';
import { TwitterShareComponent } from './twitter-share/twitter-share.component';
import { LinkedinShareComponent } from './linkedin-share/linkedin-share.component';

@NgModule({
  declarations: [
    FacebookShareComponent,
    TwitterShareComponent,
    LinkedinShareComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    FacebookShareComponent,
    TwitterShareComponent,
    LinkedinShareComponent
  ]
})
export class SocialSharesModule { }
