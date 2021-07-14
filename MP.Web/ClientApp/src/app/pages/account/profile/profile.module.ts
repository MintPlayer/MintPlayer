import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { ProfileRoutingModule } from './profile-routing.module';
import { ProfileComponent } from './profile.component';
import { SocialLoginsModule } from '../../../components/social-logins/social-logins.module';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';


@NgModule({
  declarations: [ProfileComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    CardModule,
    ForDirectiveModule,
    SocialLoginsModule,
    ProfileRoutingModule
  ]
})
export class ProfileModule { }
