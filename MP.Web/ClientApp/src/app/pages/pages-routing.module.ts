import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IsLoggedInGuard } from '../guards/is-logged-in/is-logged-in.guard';

const routes: Routes = [
  { path: '', loadChildren: () => import('./home/home.module').then(m => m.HomeModule) },
  { path: 'search', loadChildren: () => import('./search/search.module').then(m => m.SearchModule) },
  { path: 'search/:searchTerm', loadChildren: () => import('./search/search.module').then(m => m.SearchModule) },
  { path: 'account', loadChildren: () => import('./account/account.module').then(m => m.AccountModule) },
  { path: 'person', loadChildren: () => import('./person/person.module').then(m => m.PersonModule) },
  { path: 'artist', loadChildren: () => import('./artist/artist.module').then(m => m.ArtistModule) },
  { path: 'song', loadChildren: () => import('./song/song.module').then(m => m.SongModule) },
  { path: 'playlist', loadChildren: () => import('./playlist/playlist.module').then(m => m.PlaylistModule) },
  { path: 'mediumtype', loadChildren: () => import('./medium-type/medium-type.module').then(m => m.MediumTypeModule) },
  { path: 'tag/category', loadChildren: () => import('./tag-category/tag-category.module').then(m => m.TagCategoryModule) },
  { path: 'community', loadChildren: () => import('./community/community.module').then(m => m.CommunityModule) },
  { path: 'gdpr', loadChildren: () => import('./gdpr/gdpr.module').then(m => m.GdprModule) },
  { path: 'fetch', loadChildren: () => import('./fetch/fetch.module').then(m => m.FetchModule) },
  { path: '**', loadChildren: () => import('./not-found/not-found.module').then(m => m.NotFoundModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PagesRoutingModule { }
