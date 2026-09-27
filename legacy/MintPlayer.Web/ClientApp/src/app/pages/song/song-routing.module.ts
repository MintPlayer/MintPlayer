import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';
import { HasChangesGuard } from '../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', loadChildren: () => import('./list/list.module').then(m => m.ListModule) },
  { path: 'create', loadChildren: () => import('./create/create.module').then(m => m.CreateModule), canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard]  },
  { path: ':id/:title', loadChildren: () => import('./show/show.module').then(m => m.ShowModule) },
  { path: ':id/:title/edit', loadChildren: () => import('./edit/edit.module').then(m => m.EditModule), canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: ':id/:title/sync', loadChildren: () => import('./sync/sync.module').then(m => m.SyncModule), canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: 'favorite', loadChildren: () => import('./favorite/favorite.module').then(m => m.FavoriteModule), canActivate: [IsLoggedInGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SongRoutingModule { }
