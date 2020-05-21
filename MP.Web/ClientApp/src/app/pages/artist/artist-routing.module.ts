import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { FavoriteComponent } from './favorite/favorite.component';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';
import { HasChangesGuard } from '../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', component: ListComponent },
  { path: 'create', component: CreateComponent, canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: ':id/:name/edit', component: EditComponent, canActivate: [IsLoggedInGuard], canDeactivate: [HasChangesGuard] },
  { path: ':id/:name', component: ShowComponent },
  { path: 'favorite', component: FavoriteComponent, canActivate: [IsLoggedInGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ArtistRoutingModule { }
