import { HttpHeaders } from '@angular/common/http';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Playlist, PlaylistAccessibility, PlaylistService, Song } from '@mintplayer/ng-client';
import { Component, OnInit, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer, OnDestroy } from '@angular/core';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
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
    private router: AdvancedRouter,
    private enumHelper: EnumHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers
  ) {
    this.accessibilities = this.enumHelper.getItems(PlaylistAccessibility);
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
    accessibility: PlaylistAccessibility.Private,
    user: null
  };

  public accessibilities: EnumItem[] = [];
  public accessibilitySelected(accessibility: number) {
    this.playlist.accessibility = PlaylistAccessibility[PlaylistAccessibility[accessibility]];
  }

  removeTrack(track: Song) {
    this.playlist.tracks.splice(this.playlist.tracks.indexOf(track), 1);
    return false;
  }

  trackDropped(event: CdkDragDrop<string[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex);
    }
  }

  savePlaylist() {
    this.playlistService.createPlaylist(this.playlist).then((playlist) => {
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

  ngOnInit() {
    this.playlistDiffer = this.differs.find(this.playlist).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
  }
}
