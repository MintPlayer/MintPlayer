import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { ProfileRoutingModule } from './profile-routing.module';
import { ProfileComponent } from './profile.component';
import { SocialLoginsModule } from '../../../components/social-logins/social-logins.module';


@NgModule({
  declarations: [ProfileComponent],
  imports: [
    CommonModule,
    TranslateModule,
    SocialLoginsModule,
    ProfileRoutingModule
  ]
})
export class ProfileModule { }
