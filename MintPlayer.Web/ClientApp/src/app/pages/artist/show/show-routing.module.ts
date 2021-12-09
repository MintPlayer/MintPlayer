import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ShowComponent } from './show.component';

const routes: Routes = [{ path: '', component: ShowComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ShowRoutingModule { }
