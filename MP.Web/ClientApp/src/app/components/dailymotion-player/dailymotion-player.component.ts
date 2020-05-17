/// <reference path="../../../../types/dailymotion/index.d.ts" />

import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { DailyMotionHelper } from '../../helpers/dailymotion-api.helper';
import { BehaviorSubject } from 'rxjs';
import { SongProgress } from '../../entities/song-progress';

@Component({
  selector: 'app-dailymotion-player',
  templateUrl: './dailymotion-player.component.html',
  styleUrls: ['./dailymotion-player.component.scss']
})
export class DailymotionPlayerComponent implements OnInit, OnDestroy, AfterViewInit {

  constructor(private dailyMotionHelper: DailyMotionHelper) {
  }

  //#region width
  private _width: number;
  get width() {
    return this._width;
  }
  @Input() set width(value: number) {
    this._width = value;
    this.updateSize();
  }
  //#endregion
  //#region height
  private _height: number;
  get height() {
    return this._height;
  }
  @Input() set height(value: number) {
    this._height = value;
    this.updateSize();
  }
  //#endregion
  //#region autoplay
  private _autoplay: boolean;
  get autoplay() {
    return this._autoplay;
  }
  @Input() set autoplay(value: boolean) {
    this._autoplay = value;
  }
  //#endregion
  //#region mute
  get mute() {
    return this.player.muted;
  }
  set mute(value: boolean) {
    this.player.setMuted(value);
  }
  //#endregion
  //#region Volume
  get volume() {
    return this.player.volume;
  }
  set volume(value: number) {
    this.player.setVolume(value);
  }
  @Output() volumeChange: EventEmitter<number> = new EventEmitter();
  //#endregion

  @Output() stateChange: EventEmitter<DM.PlayerState> = new EventEmitter();

  private progressTimer: NodeJS.Timer;
  private player: DM.Player;
  private isPlayerReady = new BehaviorSubject<boolean>(false);

  ngOnInit() {
  }

  ngOnDestroy() {
  }

  @ViewChild('player') element: ElementRef<HTMLDivElement>;

  private updateSize() {
    if ((this.player !== null) && (this.player !== undefined)) {
      this.player.width = this._width;
      this.player.height = this._height;
    }
  }

  ngAfterViewInit() {
    this.dailyMotionHelper.apiReady.subscribe((ready) => {
      if (ready && !this.player) {
        console.log('Creating DailyMotion player');
        this.player = DM.player(this.element.nativeElement, {
          //video: 'x2yhuhb',
          width: String(this._width),
          height: String(this._height),
          params: {
            'autoplay': this._autoplay,
            'queue-autoplay-next': false,
            'queue-enable': false,
            'mute': false
          },
          events: {
            apiready: () => {
              this.isPlayerReady.next(true);
            },
            play: () => {
              this.stateChange.emit(DM.PlayerState.PLAYING);
              if (this.progressTimer === null) {
                this.progressTimer = setInterval(() => {
                  this.progressChange.emit({
                    currentTime: this.player.currentTime,
                    total: this.player.duration
                  });
                }, 100);
              }
            },
            pause: () => {
              this.stateChange.emit(DM.PlayerState.PAUSED);
              clearInterval(this.progressTimer);
              this.progressTimer = null;
            },
            end: () => {
              this.stateChange.emit(DM.PlayerState.ENDED);
              clearInterval(this.progressTimer);
              this.progressTimer = null;
            }
          }
        });
        this.player.onvolumechange = () => {
          this.volumeChange.emit(this.player.volume);
        };
        (<any>window).dailyMotionPlayer = this.player;
      }
    })
  }

  public playSong(dailyMotionId: string) {
    if (this.isPlayerReady.value) {
      this.player.load({ video: dailyMotionId });
    } else {
      this.isPlayerReady.subscribe((value) => {
        if (value === true) {
          this.player.load({ video: dailyMotionId });
        }
      })
    }
  }

  public play() {
    this.player.play();
  }

  public pause() {
    this.player.pause();
  }

  //#region Position
  public get position() {
    return this.player.currentTime;
  }
  public set position(value: number) {
    this.player.seek(value);
  }
  //#endregion

  get progress() {
    return this.player.currentTime / this.player.duration;
  }

  @Output()
  public progressChange: EventEmitter<SongProgress> = new EventEmitter();

}
