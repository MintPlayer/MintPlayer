import { Component, OnInit, Inject, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { HttpErrorResponse } from '@angular/common/http';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PlayButtonClickedEvent } from '../../../events/play-button-clicked.event';
import { ePlaylistPlaybutton } from '../../../enums/ePlaylistPlayButton';
import { Playlist, PlaylistAccessibility, PlaylistService } from '@mintplayer/ng-client';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class PlaylistShowComponent implements OnInit {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('PLAYLIST') playlistInj: Playlist,
    private playlistService: PlaylistService,
    private nativeRouter: Router,
    private router: AdvancedRouter,
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
          this.router.navigate(['/account', 'login'], {
            queryParams: {
              return: this.nativeRouter.url
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
      this.router.navigate(['playlist']);
    }).catch((error) => {
      console.error('Could not delete playlist', error);
    });
  }

  public playbuttonClicked: Subject<PlayButtonClickedEvent> = new EventEmitter<PlayButtonClickedEvent>();
  playlistPlaybuttons = ePlaylistPlaybutton;
  onPlaybuttonClicked(button: ePlaylistPlaybutton) {
    this.playbuttonClicked.next(new PlayButtonClickedEvent({
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
    accessibility: PlaylistAccessibility.Private,
    user: null
  };
}
