import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';

const routes: Routes = [
  { path: 'login', loadChildren: () => import('./login/login.module').then(m => m.LoginModule) },
  { path: 'register', loadChildren: () => import('./register/register.module').then(m => m.RegisterModule) },
  { path: 'profile', loadChildren: () => import('./profile/profile.module').then(m => m.ProfileModule), canActivate: [IsLoggedInGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountRoutingModule { }
