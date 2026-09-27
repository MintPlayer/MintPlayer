import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { QrCodeModule } from 'ng-qrcode';

import { ProfileRoutingModule } from './profile-routing.module';
import { ProfileComponent } from './profile.component';
import { SocialLoginsModule } from '../../../components/social-logins/social-logins.module';
import { CardModule } from '../../../controls/card/card.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';
import { ToggleButtonModule } from '../../../controls/toggle-button/toggle-button.module';
import { ModalModule } from '../../../components/modal/modal.module';


@NgModule({
  declarations: [ProfileComponent],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    QrCodeModule,
    CardModule,
    ModalModule,
    ToggleButtonModule,
    ForDirectiveModule,
    SocialLoginsModule,
    ProfileRoutingModule
  ]
})
export class ProfileModule { }
