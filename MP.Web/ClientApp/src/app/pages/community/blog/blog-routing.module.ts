import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ListComponent } from './list/list.component';
import { ShowComponent } from './show/show.component';
import { CreateComponent } from './create/create.component';
import { EditComponent } from './edit/edit.component';
import { IsInRoleGuard } from '../../../guards/is-in-role/is-in-role.guard';
import { HasChangesGuard } from '../../../guards/has-changes/has-changes.guard';

const routes: Routes = [
  { path: '', component: ListComponent },
  { path: 'create', component: CreateComponent, canActivate: [IsInRoleGuard], canDeactivate: [HasChangesGuard], data: { roles: ['Blogger'] } },
  { path: ':id/:name', component: ShowComponent },
  { path: ':id/:name/edit', component: EditComponent, canActivate: [IsInRoleGuard], canDeactivate: [HasChangesGuard], data: { roles: ['Blogger'] } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BlogRoutingModule { }
