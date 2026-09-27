import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';
import { HasChangesGuard } from '../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', loadChildren: () => import('./list/list.module').then(m => m.ListModule), canActivate: [IsLoggedInGuard]},
  { path: 'create', loadChildren: () => import('./create/create.module').then(m => m.CreateModule), canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard]  },
  { path: ':id/:name/edit', loadChildren: () => import('./edit/edit.module').then(m => m.EditModule), canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: ':id/:name', loadChildren: () => import('./show/show.module').then(m => m.ShowModule) },
  { path: 'public', loadChildren: () => import('./public/public.module').then(m => m.PublicModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PlaylistRoutingModule { }
