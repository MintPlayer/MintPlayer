/// <reference path="../../../../node_modules/@types/youtube/index.d.ts" />

import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { YoutubeHelper } from '../../helpers/youtube-api.helper';
import { SongProgress } from '../../entities/song-progress';
import { Song } from '../../entities/song';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'youtube-player',
  templateUrl: './youtube-player.component.html',
  styleUrls: ['./youtube-player.component.scss']
})
export class YoutubePlayerComponent implements OnInit, OnDestroy {
  constructor(private youtubeHelper: YoutubeHelper, private ref: ChangeDetectorRef) {
  }

  @Input() domId: string;
  //#region width
  private _width: number;
  get width() {
    return this._width;
  }
  @Input() set width(value: number) {
    this._width = value;
    if ((this.player !== null) && (this.player !== undefined)) {
      this.player.setSize(this._width, this._height);
    }
  }
  //#endregion
  //#region height
  private _height: number;
  get height() {
    return this._height;
  }
  @Input() set height(value: number) {
    this._height = value;
    if ((this.player !== null) && (this.player !== undefined)) {
      this.player.setSize(this._width, this._height);
    }
  }
  //#endregion
  //#region autoplay
  private _autoplay: number;
  get autoplay() {
    return this._autoplay;
  }
  @Input() set autoplay(value: number) {
    this._autoplay = value;
  }
  //#endregion
  //#region mute
  get mute() {
    return this.player.isMuted();
  }
  set mute(value: boolean) {
    if (value === true) {
      this.player.mute();
    } else {
      this.player.unMute();
    }
  }
  //#endregion
  //#region volume
  private _oldVolume: number = null;
  get volume() {
    try {
      return this.player.getVolume();
    } catch (ex) {
      return null;
    }
  }
  set volume(value: number) {
    if (value !== null) {
      this.player.setVolume(value);
    }
  }
  @Output() volumeChange: EventEmitter<number> = new EventEmitter();
  //#endregion

  @Output() stateChange: EventEmitter<YT.PlayerState> = new EventEmitter();

  @Output() previousPressed: EventEmitter<any> = new EventEmitter();
  @Output() nextPressed: EventEmitter<any> = new EventEmitter();

  private progressTimer: NodeJS.Timer;
  private volumeTimer: NodeJS.Timer;
  private player: YT.Player;
  private isPlayerReady = new BehaviorSubject<boolean>(false);

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.youtubeHelper.apiReady.subscribe((ready) => {
      if (ready && !this.player) {
        this.player = new YT.Player(this.domId, {
          width: this.width,
          height: this.height,
          playerVars: {
            autoplay: this._autoplay
          },
          events: {
            onReady: (event) => {
              this.isPlayerReady.next(true);
            },
            onStateChange: (state: { data: YT.PlayerState }) => {
              this.stateChange.emit(state.data);
              switch (state.data) {
                case YT.PlayerState.PLAYING:
                  this.progressTimer = setInterval(() => {
                    this.progressChange.emit({
                      currentTime: this.player.getCurrentTime(),
                      total: this.player.getDuration()
                    });
                  }, 100);
                  break;
                default:
                  //console.log('Clear timer');
                  clearInterval(this.progressTimer);
                  break;
              }
            }
          }
        });
        this.player.previousVideo = () => {
          this.previousPressed.emit();
        };
        this.player.nextVideo = () => {
          this.nextPressed.emit();
        };
        console.log('player', this.player);
        this.volumeTimer = setInterval(() => {
          try {
            let currentVolume = this.player.getVolume();
            if (this._oldVolume !== currentVolume) {
              this._oldVolume = currentVolume;
              this.volumeChange.emit(currentVolume);
            }
          } catch (ex) {
          }
        }, 250);
      }
    });
  }

  ngOnDestroy() {
    clearInterval(this.volumeTimer);
  }

  static mediaInit = false;
  public playSong(youtubeId: string) {
    if (this.isPlayerReady.value) {
      this.player.loadVideoById(youtubeId);
    } else {
      console.log('Player not yet ready');
      this.isPlayerReady.subscribe((value) => {
        if (value === true) {
          this.player.loadVideoById(youtubeId);
        }
      });
    }
    if (!YoutubePlayerComponent.mediaInit) {
      YoutubePlayerComponent.mediaInit = true;
      let frame = <HTMLIFrameElement>document.getElementById(this.domId);

      frame.contentWindow.navigator.mediaSession.setActionHandler('previoustrack', () => {
        this.previousPressed.emit();
      });
      frame.contentWindow.navigator.mediaSession.setActionHandler('nexttrack', () => {
        this.nextPressed.emit();
      });
    }
  }

  public play() {
    this.player.playVideo();
  }

  public pause() {
    this.player.pauseVideo();
  }

  public stop() {
    this.player.stopVideo();
  }

  public previous() {
    this.player.previousVideo();
  }

  public next() {
    this.player.nextVideo();
  }

  //#region Shuffle - Not used
  private _shuffle: boolean = false;
  public get shuffle() {
    return this._shuffle;
  }
  public set shuffle(value: boolean) {
    this.player.setShuffle(this._shuffle = value);
  }
  //#endregion
  //#region Position
  public get position() {
    return this.player.getCurrentTime();
  }
  public set position(value: number) {
    this.player.seekTo(value, true);
  }
  //#endregion
  //#region Playlist - Not used
  public addToPlaylist(song: Song) {
    this.player.cueVideoById(song.playerInfo.id);
  }
  //#endregion

  get progress() {
    return this.player.getCurrentTime() / this.player.getDuration();
  }

  @Output()
  public progressChange: EventEmitter<SongProgress> = new EventEmitter();
}
