import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AccountRoutingModule } from './account-routing.module';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ProfileComponent } from './profile/profile.component';
import { SocialLoginsModule } from '../../components/social-logins/social-logins.module';
import { DirectivesModule } from '../../directives/directives.module';


@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    ProfileComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    DirectivesModule,
    SocialLoginsModule,
    AccountRoutingModule
  ]
})
export class AccountModule { }
