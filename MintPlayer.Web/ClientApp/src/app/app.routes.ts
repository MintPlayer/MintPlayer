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
      { path: 'home', title: 'Home', loadComponent: () => import('./pages/home/home').then(m => m.Home) },
      // Metadata-driven query lists + PersistentObject create/edit/detail screens. The detail screen is
      // overridden with the app's wrapper (adds per-type toolbar actions, e.g. "Play this playlist").
      ...sparkRoutes({ poDetail: () => import('./spark/app-po-detail').then(m => m.AppPoDetail) }),
    ],
  },
];
