import { Component, ElementRef, ViewChild, ChangeDetectorRef, Inject, OnInit, OnDestroy, AfterViewInit, HostListener, NgZone } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { SwUpdate } from '@angular/service-worker';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { User, AccountService } from '@mintplayer/ng-client';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PlayerProgress } from '@mintplayer/ng-player-progress';
import { PlayerState, VideoPlayerComponent } from '@mintplayer/ng-video-player';
import { Subject, Observable, BehaviorSubject, combineLatest, interval } from 'rxjs';
import { filter, map, take, takeUntil } from 'rxjs/operators';

import { eToggleButtonState } from './enums/eToggleButtonState';
import { eSidebarState } from './enums/eSidebarState';
import { LoginComponent } from './pages/account/login/login.component';
import { RegisterComponent } from './pages/account/register/register.component';
import { ShowComponent as SongShowComponent } from './pages/song/show/show.component';
import { Size } from './entities/size';
import { PlaylistShowComponent } from './pages/playlist/show/show.component';
import { PlayButtonClickedEvent } from './events/play-button-clicked.event';
import { ePlaylistPlaybutton } from './enums/ePlaylistPlayButton';
import { LinifyPipe } from './pipes/linify/linify.pipe';
import { SyncComponent } from './pages/song/sync/sync.component';
import { HreflangTagHelper } from './helpers/hreflang-tag.helper';
import { TwoFactorComponent } from './pages/account/two-factor/two-factor.component';
import { SongWithMedium } from './interfaces/song-with-medium';
import { PlaylistController } from '@mintplayer/ng-playlist-controller';
import { RecoveryComponent } from './pages/account/two-factor/recovery/recovery.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
  title = 'MintPlayer';
  activeUser: User = null;
  fullWidth: boolean = false;
  isViewInited: boolean = false;
  toggleButtonState: eToggleButtonState = eToggleButtonState.auto;
  sidebarState: eSidebarState = eSidebarState.auto;
  playlistToggleButtonState: eToggleButtonState = eToggleButtonState.closed;

  playlistControl: PlaylistController<SongWithMedium | string>;
  private destroyed$ = new Subject();

  //#region Player card size
  playerSize: Size = { width: 200, height: 150 };
  @HostListener('window:resize')
  onWindowResize() {
    if (typeof window !== 'undefined') {
      if (window.innerWidth < 640) {
        this.playerSize = { width: 200, height: 150 };
      } else {
        this.playerSize = { width: 320, height: 240 };
      }
    }
  }
  //#endregion

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('USER') userInj: User,
    private accountService: AccountService,
    private ref: ChangeDetectorRef,
    private zone: NgZone,
    private swUpdate: SwUpdate,
    private metaService: Meta,
    private linifyPipe: LinifyPipe,
    private route: ActivatedRoute,
    private translateService: TranslateService,
    private hreflangTagHelper: HreflangTagHelper,
  ) {
    setTimeout(() => {
      this.playerState$.next(7);
    }, 3000);
    //#region Get user
    if (serverSide === true) {
      this.activeUser = userInj;
    } else {
      this.accountService.currentUser().then((user) => {
        this.activeUser = user;
      }).catch((error) => {
        this.activeUser = null;
      });
    }
    //#endregion
    //#region Check for updates
    if (this.swUpdate.isEnabled) {
      this.swUpdate.activated.subscribe((upd) => {
        window.location.reload();
      });
      this.swUpdate.available.subscribe((upd) => {
        this.swUpdate.activateUpdate();
      }, (error) => {
        console.error(error);
      });
      this.swUpdate.checkForUpdate().then(() => {
      }).catch((error) => {
        console.error('Could not check for app updates', error);
      });
    }
    //#endregion
    //#region Pre-compute the player size
    this.onWindowResize();
    //#endregion
    //#region Initialize PlaylistController
    this.playlistControl = new PlaylistController<SongWithMedium>();
    this.playlistControl.video$
      .pipe(takeUntil(this.destroyed$))
      .subscribe((song) => {
        if (this.isViewInited) {
          console.log('video$.next', song);
          if (song === null) {
            this.player.setUrl(null);
            setTimeout(() => {
              console.log('playerState$.next(6)');
              this.playerState$.next(6);
            }, 1000);
          } else if (typeof song === 'string') {
            this.player.setUrl(song);
          } else if (song.medium !== null) {
            this.player.setUrl(song.medium.value);
          } else if (song.song.playerInfos.length > 0) {
            this.player.setUrl(song.song.playerInfos[0].url);
          }
        }
      });

    this.playerState$
      .pipe(takeUntil(this.destroyed$))
      .subscribe((playerState) => {
        console.log('playerStateChange', playerState);
      });

    this.playerState$
      .pipe(filter((playerState) => playerState === PlayerState.ended))
      .pipe(takeUntil(this.destroyed$))
      .subscribe((playerState) => {
        this.playNextSong();
      });

    this.currentLyricsLine$ = combineLatest([this.playerState$, this.playlistControl.video$, interval(50)])
      .pipe(filter(([playerState, video, time]) => {
        return playerState === PlayerState.playing;
      }))
      .pipe(map(([playerState, video, time]) => {
        if (typeof video === 'string') {
          return null;
        } else if (video.song.lyrics.timeline === null) {
          return null;
        } else {
          const linesPassed = this.linifyPipe
            .transform(video.song.lyrics.text)
            .map((value, index) => {
              return {
                time: video.song.lyrics.timeline[index],
                line: value
              };
            }).filter((value, index) => {
              return value.time < this.songProgress.currentTime;
            });

          if (linesPassed.length > 0) {
            return linesPassed[linesPassed.length - 1].line;
          } else {
            return null;
          }
        }
      }))
      .pipe(takeUntil(this.destroyed$));
    //#endregion
    //#region Translate
    const defaultLang = 'en';
    this.translateService.setDefaultLang(defaultLang);
    this.route.queryParamMap.subscribe((params) => {
      let lang = params.get('lang');
      if (lang === null) {
        this.translateService.use(defaultLang);
      } else {
        this.translateService.use(lang);
      }
    });
    //#endregion
    //#region Viewport resize
    if (!serverSide && ('visualViewport' in window)) {
      visualViewport.addEventListener('resize', () => {
        let scaledSize = visualViewport.scale * visualViewport.height;
        document.documentElement.style.setProperty('--viewport-height', `${scaledSize}px`);
      });
    }
    //#endregion
  }

  //#region Add meta-tags
  private ogMetaTags: HTMLMetaElement[] = [];
  ngOnInit() {
    this.ogMetaTags = this.metaService.addTags([{
      property: 'og:site_name',
      content: 'MintPlayer'
    }]);
  }
  ngOnDestroy() {
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.destroyed$.next(true);
  }
  ngAfterViewInit() {
    this.isViewInited = true;
  }
  //#endregion

  updateSidebarState(state: eToggleButtonState) {
    switch (state) {
      case eToggleButtonState.open:
        this.sidebarState = eSidebarState.show;
        break;
      case eToggleButtonState.closed:
        this.sidebarState = eSidebarState.hide;
        break;
      default:
        this.sidebarState = eSidebarState.auto;
        break;
    }
  }

  collapseSidebar() {
    if (window.innerWidth < 768) {
      this.toggleButtonState = eToggleButtonState.closed;
      this.sidebarState = eSidebarState.hide;
    }
  }

  loginCompleted = (user: User) => {
    this.activeUser = user;
  }

  logoutClicked() {
    this.accountService.logout().then(() => {
      this.accountService.csrfRefresh().then(() => {
        this.activeUser = null;
      });
    }).catch((error) => {
      console.error('Could not logout', error);
    });
  }

  //#region Lyrics display
  isSyncComponent: boolean = false;
  currentLyricsLine$: Observable<string>;
  //#endregion

  //#region Playlist

  collapsePlaylistSidebar() {
    if (typeof window !== 'undefined') {
      if (window.innerWidth < 768) {
        this.playlistToggleButtonState = eToggleButtonState.closed;
      }
    }
  }

  // Current position of the current song (percentage)
  songProgress: PlayerProgress = {
    currentTime: 0,
    duration: 0
  };

  /** Add this song to the User playlist, and start playing if necessary */
  addToPlaylist = (song: SongWithMedium) => {
    this.playlistControl.addToPlaylist(song);
  }

  @ViewChild('player') player: VideoPlayerComponent;

  playerState$ = new BehaviorSubject<PlayerState>(PlayerState.unstarted);
  playerStateChanged(state: PlayerState) {
    this.playerState$.next(state);
  }

  /**
   * Updates the value of the progress bar on the SidebarComponent.
   * @param progress Current percentage of the YoutubePlayer.
   */
  playerProgressChange(progress: PlayerProgress) {
    this.songProgress = progress;
    this.ref.detectChanges();
  }

  /** Invoked when the user presses the Previous button */
  playPreviousSong() {
    this.playlistControl.previous();
  }
  playNextSong() {
    this.playlistControl.playerEnded();
    this.ref.detectChanges();
  }
  playPausePlayer() {
    this.playerState$.pipe(take(1)).subscribe((playerState) => {
      if (playerState === PlayerState.playing) {
        this.player.playerState = PlayerState.paused;
      } else {
        this.player.playerState = PlayerState.playing;
      }
    });
  }
  replayPlayer() {
    this.playlistControl.next();
  }
  //#endregion

  routingActivated(element: ElementRef) {
    this.hreflangTagHelper.setHreflangTags();

    // Login complete
    if (element instanceof LoginComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof TwoFactorComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof RecoveryComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof RegisterComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof SongShowComponent) {
      element.addToPlaylist.subscribe(this.addToPlaylist);
    } else if (element instanceof PlaylistShowComponent) {
      element.playbuttonClicked.subscribe((event: PlayButtonClickedEvent) => {
        switch (event.button) {
          case ePlaylistPlaybutton.addToQueue: {
            this.playlistControl.addToPlaylist(...event.songs.map(s => <SongWithMedium>{ song: s, medium: null }));
          } break;
          case ePlaylistPlaybutton.playNow: {
            this.playlistControl.shuffle = false;
            this.playlistControl.setPlaylist(event.songs.map(s => <SongWithMedium>{ song: s, medium: null }));
          } break;
          case ePlaylistPlaybutton.shuffle: {
            this.playlistControl.shuffle = true;
            this.playlistControl.setPlaylist(event.songs.map(s => <SongWithMedium>{ song: s, medium: null }));
          } break;
        }
      });
    } else if (element instanceof SyncComponent) {
      this.isSyncComponent = true;
    }
  }

  routingDeactivated(element: ElementRef) {
    // Login complete
    if (element instanceof LoginComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof TwoFactorComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof RecoveryComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof RegisterComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof SongShowComponent) {
      element.addToPlaylist.unsubscribe();
    } else if (element instanceof PlaylistShowComponent) {
      element.playbuttonClicked.unsubscribe();
    } else if (element instanceof SyncComponent) {
      this.isSyncComponent = false;
    }
  }
}
