import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { LoginRoutingModule } from './login-routing.module';
import { LoginComponent } from './login.component';
import { DirectivesModule } from '../../../directives/directives.module';
import { SocialLoginsModule } from '../../../components/social-logins/social-logins.module';


@NgModule({
  declarations: [LoginComponent],
  imports: [
    CommonModule,
    FormsModule,
    DirectivesModule,
    SocialLoginsModule,
    TranslateModule,
    LoginRoutingModule
  ]
})
export class LoginModule { }
