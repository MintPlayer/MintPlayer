import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'account', loadChildren: './account/account.module#AccountModule' },
  { path: 'person', loadChildren: './person/person.module#PersonModule' },
  { path: 'artist', loadChildren: './artist/artist.module#ArtistModule' },
  { path: 'song', loadChildren: './song/song.module#SongModule' },
  { path: 'mediumtype', loadChildren: './medium-type/medium-type.module#MediumTypeModule' },
  { path: 'search', component: SearchComponent, pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PagesRoutingModule { }
