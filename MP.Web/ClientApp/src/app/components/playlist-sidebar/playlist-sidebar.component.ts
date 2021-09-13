import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PlayerProgress } from '@mintplayer/ng-player-progress';
import { PlayerState } from '@mintplayer/ng-video-player';
import { SongRemovedEvent } from '../../events/song-removed.event';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { eRepeatMode } from '../../enums/eRepeatMode';
import { SongWithMedium } from '../../interfaces/song-with-medium';

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

  playerStateValues = PlayerState;

  @Input()
  public playerState: PlayerState;

  @Input()
  public songs: (SongWithMedium | string)[];

  @Input()
  public current: SongWithMedium;

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
  isString(song: SongWithMedium | string) {
    return typeof song === 'string';
  }

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

  public removeSong($event: MouseEvent, song: SongWithMedium) {
    var index = this.songs.indexOf(song);
    //this.songs.splice(index, 1);
    $event.stopPropagation();
    this.songRemoved.emit(new SongRemovedEvent({ index, song }));
  }

  ngOnInit() {
  }
}
