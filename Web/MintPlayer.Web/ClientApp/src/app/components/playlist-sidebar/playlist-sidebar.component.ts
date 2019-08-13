import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Song } from '../../interfaces/song';

@Component({
  selector: 'playlist-sidebar',
  templateUrl: './playlist-sidebar.component.html',
  styleUrls: ['./playlist-sidebar.component.scss']
})
export class PlaylistSidebarComponent implements OnInit {
  constructor(private router: Router) {
  }

  @Input()
  public songs: Song[];

  @Input()
  public current: Song;

  public openSong(song: Song) {
    this.router.navigate(['song', song.id]);
  }

  public removeSong($event: MouseEvent, song: Song) {
    this.songs.splice(this.songs.indexOf(song), 1);
    $event.stopPropagation();
  }

  ngOnInit() {
  }
}
