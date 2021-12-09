import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { PlaylistCreateComponent } from './create.component';

const routes: Routes = [{ path: '', component: PlaylistCreateComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CreateRoutingModule { }
