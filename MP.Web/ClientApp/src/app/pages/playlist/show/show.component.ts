import { Component, OnInit, Inject, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Playlist } from '../../../entities/playlist';
import { PlayButtonClickedEvent } from '../../../events/play-button-clicked.event';
import { ePlaylistPlaybutton } from '../../../enums/ePlaylistPlayButton';
import { NavigationHelper } from '../../../helpers/navigation.helper';
import { ePlaylistAccessibility } from '../../../enums/ePlaylistAccessibility';
import { HttpErrorResponse } from '@angular/common/http';
import { STATUS_CODES } from 'http';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class PlaylistShowComponent implements OnInit {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('PLAYLIST') playlistInj: Playlist,
    private playlistService: PlaylistService,
    private navigation: NavigationHelper,
    private router: Router,
    private route: ActivatedRoute,
    private titleService: Title
  ) {
    if (serverSide === true) {
      this.setPlaylist(playlistInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPlaylist(id);
    }
  }

  private loadPlaylist(id: number) {
    this.playlistService.getPlaylist(id, true).then((playlist) => {
      this.setPlaylist(playlist);
    }).catch((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401: // Unauthorized
          this.navigation.navigate(['/account', 'login'], {
            queryParams: {
              return: this.router.url
            }
          });
          break;
        case 403: // Forbidden
          break;
        case 404:
          break;
      }
      console.error('Could not get playlist', error);
    });
  }

  private setPlaylist(playlist: Playlist) {
    this.playlist = playlist;
    if (this.playlist != null) {
      this.titleService.setTitle(`Playlist ${playlist.description}`);
    }
  }

  deletePlaylist() {
    this.playlistService.deletePlaylist(this.playlist).then(() => {
      this.navigation.navigate(['playlist']);
    }).catch((error) => {
      console.error('Could not delete playlist', error);
    });
  }

  public playbuttonClicked: EventEmitter<PlayButtonClickedEvent> = new EventEmitter<PlayButtonClickedEvent>();
  playlistPlaybuttons = ePlaylistPlaybutton;
  onPlaybuttonClicked(button: ePlaylistPlaybutton) {
    this.playbuttonClicked.emit(new PlayButtonClickedEvent({
      button: button,
      songs: this.playlist.tracks
    }));
  }

  ngOnInit() {
  }

  playlist: Playlist = {
    id: 0,
    description: '',
    tracks: [],
    accessibility: ePlaylistAccessibility.Private,
    user: null
  };
}
