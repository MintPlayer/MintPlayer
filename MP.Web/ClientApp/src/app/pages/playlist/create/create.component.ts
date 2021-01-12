import { Component, OnInit, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer, OnDestroy } from '@angular/core';
import { Playlist } from '../../../entities/playlist';
import { Song } from '../../../entities/song';
import { HttpHeaders } from '@angular/common/http';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Router } from '@angular/router';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { NavigationHelper } from '../../../helpers/navigation.helper';
import { ePlaylistAccessibility } from '../../../enums/ePlaylistAccessibility';
import { EnumHelper } from '../../../helpers/enum.helper';
import { EnumItem } from '../../../entities/enum-item';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class PlaylistCreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    private playlistService: PlaylistService,
    private navigation: NavigationHelper,
    private enumHelper: EnumHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers
  ) {
    this.accessibilities = this.enumHelper.getItems(ePlaylistAccessibility);
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
    accessibility: ePlaylistAccessibility.Private,
    user: null
  };

  public accessibilities: EnumItem[] = [];
  public accessibilitySelected(accessibility: number) {
    this.playlist.accessibility = ePlaylistAccessibility[ePlaylistAccessibility[accessibility]];
  }

  removeTrack(track: Song) {
    this.playlist.tracks.splice(this.playlist.tracks.indexOf(track), 1);
    return false;
  }

  savePlaylist() {
    this.playlistService.createPlaylist(this.playlist).then((playlist) => {
      this.hasChanges = false;
      this.navigation.navigate(['/playlist', playlist.id, this.slugifyHelper.slugify(playlist.description)]);
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

  ngOnInit() {
    this.playlistDiffer = this.differs.find(this.playlist).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
  }
}
