import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { ShowComponent } from './show/show.component';
import { IsLoggedInGuard } from '../../../guards/is-logged-in/is-logged-in.guard';

const routes: Routes = [
  { path: 'create', component: CreateComponent, canActivate: [IsLoggedInGuard] },
  { path: ':id/edit', component: EditComponent, canActivate: [IsLoggedInGuard] },
  { path: ':id', component: ShowComponent },
  { path: ':id/tags/create', component: CreateComponent, canActivate: [IsLoggedInGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TagRoutingModule { }
