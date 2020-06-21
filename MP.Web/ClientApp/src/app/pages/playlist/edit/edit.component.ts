import { Component, OnInit, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Song } from '../../../entities/song';
import { Playlist } from '../../../entities/playlist';
import { Title } from '@angular/platform-browser';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class PlaylistEditComponent implements OnInit, DoCheck, HasChanges {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('PLAYLIST') playlistInj: Playlist,
    private playlistService: PlaylistService,
    private router: Router,
    private route: ActivatedRoute,
    private titleService: Title,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers
  ) {
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
      this.oldPlaylistDescription = playlist.description;
    }
    this.playlistDiffer = this.differs.find(this.playlist).create();
    setTimeout(() => this.hasChanges = false);
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
      this.hasChanges = false;
      this.router.navigate(['/playlist', playlist.id, this.slugifyHelper.slugify(playlist.description)]);
    }).catch((error) => {
      debugger;
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private playlistDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.playlistDiffer !== null) {
      const changes = this.playlistDiffer.diff(this.playlist);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion
}
