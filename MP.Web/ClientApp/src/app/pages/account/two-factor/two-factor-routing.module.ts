import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TwoFactorComponent } from './two-factor.component';

const routes: Routes = [{ path: '', component: TwoFactorComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TwoFactorRoutingModule { }
