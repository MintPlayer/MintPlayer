import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PerformComponent } from './perform.component';

const routes: Routes = [{ path: '', component: PerformComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PerformRoutingModule { }
