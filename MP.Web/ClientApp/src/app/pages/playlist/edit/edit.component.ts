import { Component, OnInit, Inject, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Song } from '../../../entities/song';
import { Playlist } from '../../../entities/playlist';
import { Title } from '@angular/platform-browser';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class PlaylistEditComponent implements OnInit {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('PLAYLIST') playlistInj: Playlist, private playlistService: PlaylistService, private router: Router, private route: ActivatedRoute, private titleService: Title, private slugifyHelper: SlugifyHelper) {
    if (serverSide === false) {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPlaylist(id);
    }
  }

  private loadPlaylist(id: number) {
    this.playlistService.getPlaylist(id, true).then((playlist) => {
      this.setPlaylist(playlist);
    }).catch((error) => {
      console.error('Could not get playlist', error);
    });
  }

  private setPlaylist(playlist: Playlist) {
    this.playlist = playlist;
    if (this.playlist != null) {
      this.titleService.setTitle(`Playlist ${playlist.description}`);
    }
  }

  ngOnInit() {
  }

  songSuggestHttpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  onSuggestionClicked(suggestion: Song) {
    this.playlist.tracks.push(suggestion);
  }

  oldPlaylistDescription: string = '';
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
    this.playlistService.updatePlaylist(this.playlist).then((playlist) => {
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
