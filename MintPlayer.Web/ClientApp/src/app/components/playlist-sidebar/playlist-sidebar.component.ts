import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PlayerProgress } from '@mintplayer/ng-player-progress';
import { EPlayerState } from '@mintplayer/ng-video-player';
import { SongRemovedEvent } from '../../events/song-removed.event';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { eRepeatMode } from '../../enums/eRepeatMode';
import { SongWithMedium } from '../../interfaces/song-with-medium';
import { Subject } from 'rxjs';
import { VideoUrl } from '../../interfaces/video-url';

@Component({
  selector: 'playlist-sidebar',
  templateUrl: './playlist-sidebar.component.html',
  styleUrls: ['./playlist-sidebar.component.scss'],
  providers: [
    SlugifyPipe
  ]
})
export class PlaylistSidebarComponent implements OnInit {
  constructor() {
  }

  playerStateValues = EPlayerState;

  @Input()
  public playerState: EPlayerState;

  @Input()
  public songs: (SongWithMedium | VideoUrl)[];

  //#region Current
  private _current: SongWithMedium | VideoUrl;
  public get current() {
    return this._current;
  }
  @Input() public set current(value: SongWithMedium | VideoUrl) {
    this._current = value;
    if (value === null) {
      this.currentVideoText = null;
    } else if ('url' in value) {
      this.currentVideoText = value.url;
    } else {
      this.currentVideoText = value.song.description;
    }
  }
  //#endregion

  @Input()
  public songProgress: PlayerProgress;

  @Output()
  public songRemoved: EventEmitter<SongRemovedEvent> = new EventEmitter();

  @Output()
  public previousClicked: EventEmitter<any> = new EventEmitter();

  @Output()
  public nextClicked: EventEmitter<any> = new EventEmitter();

  @Output()
  public playPauseClicked: EventEmitter<any> = new EventEmitter();

  @Output()
  public replayClicked: EventEmitter<any> = new EventEmitter();

  @Output()
  public songClicked: EventEmitter<SongWithMedium> = new EventEmitter();

  @Output()
  public addVideoUrlClicked: EventEmitter<any> = new EventEmitter();

  onPreviousClicked() {
    this.previousClicked.emit();
  }
  onNextClicked() {
    this.nextClicked.emit();
  }
  onPlayPauseClicked() {
    this.playPauseClicked.emit();
  }
  onReplay() {
    this.replayClicked.emit();
  }
  onSongClicked(song: SongWithMedium) {
    this.songClicked.emit(song);
  }
  onAddVideoUrl() {
    this.addVideoUrlClicked.emit();
  }

  isVideoUrl(song: SongWithMedium | VideoUrl) {
    if ('url' in song) {
      return true;
    } else {
      return false;
    }
  }
  currentVideoText: string = null;

  //#region isRandom
  @Input() isRandom: boolean;
  @Output() isRandomChange: EventEmitter<boolean> = new EventEmitter();

  onToggleIsRandom() {
    this.isRandom = !this.isRandom;
    this.isRandomChange.emit(this.isRandom);
  }
  //#endregion
  //#region repeatMode
  repeatModes = eRepeatMode;
  @Input() repeatMode: eRepeatMode;
  @Output() repeatModeChange: EventEmitter<eRepeatMode> = new EventEmitter();
  onToggleRepeat() {
    if (++this.repeatMode === 3) {
      this.repeatMode = 0;
    }
    this.repeatModeChange.emit(this.repeatMode);
  }
  //#endregion

  public removeSong($event: MouseEvent, song: SongWithMedium | VideoUrl) {
    var index = this.songs.indexOf(song);
    //this.songs.splice(index, 1);
    $event.stopPropagation();
    this.songRemoved.emit(new SongRemovedEvent({ index, song }));
  }

  //videoUrlToAdd: string = '';
  //isRequestingPlaylistUrl$ = new Subject<boolean>();
  ngOnInit() {
  }
}
