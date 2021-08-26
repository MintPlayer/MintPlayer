import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdvancedRouterModule } from '@mintplayer/ng-router';

import { LoginRoutingModule } from './login-routing.module';
import { LoginComponent } from './login.component';
import { SocialLoginsModule } from '../../../components/social-logins/social-logins.module';
import { PipesModule } from '../../../pipes/pipes.module';
import { ForDirectiveModule } from '../../../directives/for/for-directive.module';


@NgModule({
  declarations: [LoginComponent],
  imports: [
    CommonModule,
    FormsModule,
    PipesModule,
    ForDirectiveModule,
    AdvancedRouterModule,
    SocialLoginsModule,
    TranslateModule,
    LoginRoutingModule
  ]
})
export class LoginModule { }
