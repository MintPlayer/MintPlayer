import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { SyncComponent } from './sync.component';

const routes: Routes = [{ path: '', component: SyncComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SyncRoutingModule { }
