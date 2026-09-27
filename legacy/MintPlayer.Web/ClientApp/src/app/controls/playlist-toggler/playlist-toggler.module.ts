import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlaylistTogglerComponent } from './playlist-toggler.component';

@NgModule({
  declarations: [
    PlaylistTogglerComponent
  ],
  imports: [
    CommonModule,
  ],
  exports: [
    PlaylistTogglerComponent
  ]
})
export class PlaylistTogglerModule { }
