import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IsInRoleGuard } from '../../../guards/is-in-role/is-in-role.guard';
import { HasChangesGuard } from '../../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', loadChildren: () => import('./list/list.module').then(m => m.ListModule) },
  { path: ':id/:name', loadChildren: () => import('./show/show.module').then(m => m.ShowModule) },
  { path: 'create', loadChildren: () => import('./create/create.module').then(m => m.CreateModule), canActivate: [IsInRoleGuard], canDeactivate: [HasChangesGuard], data: { roles: ['Blogger'] } },
  { path: ':id/:name/edit', loadChildren: () => import('./edit/edit.module').then(m => m.EditModule), canActivate: [IsInRoleGuard], canDeactivate: [HasChangesGuard], data: { roles: ['Blogger'] } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BlogRoutingModule { }
