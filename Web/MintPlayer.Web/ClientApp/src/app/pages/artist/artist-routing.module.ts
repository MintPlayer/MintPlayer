import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { IsLoggedInGuard } from '../../guards/is-logged-in/is-logged-in.guard';

const routes: Routes = [
  { path: '', component: ListComponent },
  { path: 'create', component: CreateComponent, canActivate: [IsLoggedInGuard] },
  { path: ':id/edit', component: EditComponent, canActivate: [IsLoggedInGuard] },
  { path: ':id', component: ShowComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ArtistRoutingModule { }
