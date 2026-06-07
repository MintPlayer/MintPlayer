import { Routes } from '@angular/router';
import { sparkAuthRoutes } from '@mintplayer/ng-spark-auth/routes';
import { sparkRoutes } from '@mintplayer/ng-spark/routes';
import { Shell } from './shell/shell';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      ...sparkAuthRoutes(),
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', loadComponent: () => import('./pages/home/home').then(m => m.Home) },
      // Metadata-driven query lists + PersistentObject create/edit/detail screens.
      ...sparkRoutes(),
    ],
  },
];
