import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PlaylistListComponent } from './list/list.component';
import { PlaylistCreateComponent } from './create/create.component';
import { PlaylistEditComponent } from './edit/edit.component';
import { PlaylistShowComponent } from './show/show.component';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';
import { HasChangesGuard } from '../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', component: PlaylistListComponent, canActivate: [IsLoggedInGuard] },
  { path: 'create', component: PlaylistCreateComponent, canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: ':id/:name/edit', component: PlaylistEditComponent, canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard]},
  { path: ':id/:name', component: PlaylistShowComponent, canActivate: [IsLoggedInGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PlaylistRoutingModule { }
