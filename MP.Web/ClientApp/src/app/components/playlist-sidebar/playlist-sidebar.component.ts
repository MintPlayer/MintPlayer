import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Song } from '@mintplayer/ng-client';
import { PlayerProgress } from '@mintplayer/ng-player-progress';
import { SongRemovedEvent } from '../../events/song-removed.event';
import { SlugifyPipe } from '../../pipes/slugify/slugify.pipe';
import { eRepeatMode } from '../../enums/eRepeatMode';

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

  @Input()
  public playerState: YT.PlayerState;

  @Input()
  public songs: Song[];

  @Input()
  public current: Song;

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
  public songClicked: EventEmitter<Song> = new EventEmitter();

  onPreviousClicked() {
    this.previousClicked.emit();
  }
  onNextClicked() {
    this.nextClicked.emit();
  }
  onPlayPauseClicked() {
    this.playPauseClicked.emit();
  }
  onSongClicked(song: Song) {
    this.songClicked.emit(song);
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

  public removeSong($event: MouseEvent, song: Song) {
    var index = this.songs.indexOf(song);
    //this.songs.splice(index, 1);
    $event.stopPropagation();
    console.log('emit songRemoved', index, song);
    this.songRemoved.emit(new SongRemovedEvent({ index, song }));
  }

  ngOnInit() {
  }
}
