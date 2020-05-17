import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { IsLoggedInGuard } from '../guards/is-logged-in/is-logged-in.guard';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'search', component: SearchComponent },
  { path: 'search/:searchTerm', component: SearchComponent },
  { path: 'account', loadChildren: () => import('./account/account.module').then(m => m.AccountModule) },
  { path: 'person', loadChildren: () => import('./person/person.module').then(m => m.PersonModule) },
  { path: 'artist', loadChildren: () => import('./artist/artist.module').then(m => m.ArtistModule) },
  { path: 'song', loadChildren: () => import('./song/song.module').then(m => m.SongModule) },
  { path: 'playlist', loadChildren: () => import('./playlist/playlist.module').then(m => m.PlaylistModule), canActivate: [IsLoggedInGuard] },
  { path: 'mediumtype', loadChildren: () => import('./medium-type/medium-type.module').then(m => m.MediumTypeModule) },
  { path: 'tag/category', loadChildren: () => import('./tag-category/tag-category.module').then(m => m.TagCategoryModule) },
  { path: 'community', loadChildren: () => import('./community/community.module').then(m => m.CommunityModule) },
  { path: 'gdpr', loadChildren: () => import('./gdpr/gdpr.module').then(m => m.GdprModule) },
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PagesRoutingModule { }
