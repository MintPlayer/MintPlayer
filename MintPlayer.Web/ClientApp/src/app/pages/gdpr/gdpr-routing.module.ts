import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
  { path: 'privacy-policy', loadChildren: () => import('./privacy-policy/privacy-policy.module').then(m => m.PrivacyPolicyModule) },
  { path: 'terms-of-service', loadChildren: () => import('./terms-of-service/terms-of-service.module').then(m => m.TermsOfServiceModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GdprRoutingModule { }
