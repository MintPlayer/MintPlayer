import { Component, OnInit, HostListener } from '@angular/core';
import { Playlist } from '../../../entities/playlist';
import { Song } from '../../../entities/song';
import { HttpHeaders } from '@angular/common/http';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Router } from '@angular/router';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class PlaylistCreateComponent implements OnInit {

  constructor(private playlistService: PlaylistService, private router: Router, private slugifyHelper: SlugifyHelper) {
  }

  ngOnInit() {
  }

  songSuggestHttpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  onSuggestionClicked(suggestion: Song) {
    this.playlist.tracks.push(suggestion);
  }

  playlist: Playlist = {
    id: 0,
    description: '',
    tracks: [],
    user: null
  };

  removeTrack(track: Song) {
    this.playlist.tracks.splice(this.playlist.tracks.indexOf(track), 1);
    return false;
  }

  savePlaylist() {
    this.playlistService.createPlaylist(this.playlist).then((playlist) => {
      this.router.navigate(['/playlist', playlist.id, this.slugifyHelper.slugify(playlist.description)]);
    }).catch((error) => {
      debugger;
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }
}
