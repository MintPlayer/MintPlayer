import { Component, ElementRef, ViewChild, ChangeDetectorRef, Inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { SwUpdate } from '@angular/service-worker';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { User, Song, AccountService, PlayerType } from '@mintplayer/ng-client';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PlayerProgress } from '@mintplayer/ng-player-progress';
import { PlayerState, VideoPlayerComponent } from '@mintplayer/ng-video-player';

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
import { PlaylistControl } from './helpers/playlist-control.helper';
import { SyncComponent } from './pages/song/sync/sync.component';
import { HreflangTagHelper } from './helpers/hreflang-tag.helper';
import { TwoFactorComponent } from './pages/account/two-factor/two-factor.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'MintPlayer';
  activeUser: User = null;
  fullWidth: boolean = false;
  toggleButtonState: eToggleButtonState = eToggleButtonState.auto;
  sidebarState: eSidebarState = eSidebarState.auto;
  playlistToggleButtonState: eToggleButtonState = eToggleButtonState.closed;

  playlistControl: PlaylistControl<Song>;

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
    private swUpdate: SwUpdate,
    private metaService: Meta,
    private linifyPipe: LinifyPipe,
    private route: ActivatedRoute,
    private translateService: TranslateService,
    private hreflangTagHelper: HreflangTagHelper,
  ) {
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
      console.log('Updates enabled');
      this.swUpdate.activated.subscribe((upd) => {
        console.log('Update activated');
        window.location.reload();
      });
      this.swUpdate.available.subscribe((upd) => {
        console.log('Update available');
        console.log('Activating update');
        this.swUpdate.activateUpdate();
      }, (error) => {
          console.log(error);
      });
      this.swUpdate.checkForUpdate().then(() => {
        console.log('Checking for updates');
      }).catch((error) => {
        console.log('Could not check for app updates', error);
      });
    }
    //#endregion
    //#region Pre-compute the player size
    this.onWindowResize();
    //#endregion
    //#region Initialize PlaylistController
    this.playlistControl = new PlaylistControl<Song>({
      onGetCurrentPosition: () => this.player.currentTime,
      onPlayVideo: (song) => {
        console.log('player infos', song.playerInfos);
        debugger;
        if (song.playerInfos.length > 0) {
          this.player.url = song.playerInfos[0].url;
        }
      },
      onStopVideo: () => {
        this.player.playerState = PlayerState.ended;
      }
    });
    //#endregion
    //#region Translate
    const defaultLang = 'en';
    this.translateService.setDefaultLang(defaultLang);
    this.route.queryParamMap.subscribe((params) => {
      let lang = params.get('lang');
      console.log('language', lang);
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
        document.documentElement.style.setProperty('--viewport-height', `${visualViewport.height}px`);
      });
    }
    //#endregion








    let matches = new RegExp(/http[s]{0,1}:\/\/(www\.){0,1}youtube\.com\/watch\?v=(?<id>[^&]+)/, 'g')
      .exec('https://www.youtube.com/watch?v=UqQ9NWzIiM4');

    console.log('matches', matches);






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
      this.activeUser = null;
    }).catch((error) => {
      console.error('Could not logout', error);
    });
  }

  //#region Lyrics display
  isSyncComponent: boolean = false;
  currentLyricsLine: string = null;

  private computeCurrentLyricsLine() {
    let linesPassed = this.linifyPipe
      .transform(this.playlistControl.currentVideo.lyrics.text)
      .map((value, index) => {
        return {
          time: this.playlistControl.currentVideo.lyrics.timeline[index],
          line: value
        };
      }).filter((value, index) => {
        return value.time < this.songProgress.currentTime;
      });

    if (linesPassed.length > 0) {
      this.currentLyricsLine = linesPassed[linesPassed.length - 1].line;
    } else {
      this.currentLyricsLine = null;
    }
  }
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
  addToPlaylist = (song: Song) => {
    this.playlistControl.addToPlaylist(song);
  }

  @ViewChild('player') player: VideoPlayerComponent;

  private lyricsTimer: NodeJS.Timer;
  playerState: PlayerState = PlayerState.unstarted;
  playerStateChanged(state: PlayerState) {
    this.playerState = state;
    switch (state) {
      case PlayerState.playing:
        if (this.playlistControl.currentVideo.lyrics.timeline === null) {
          this.currentLyricsLine = null;
        } else {
          this.lyricsTimer = setInterval(() => {
            this.computeCurrentLyricsLine();
          }, 100);
        }
        break;
      case PlayerState.paused:
        if (this.lyricsTimer !== null) {
          clearInterval(this.lyricsTimer);
        }
        break;
      case PlayerState.ended:
        this.playNextSong();
        break;
      default:
        clearInterval(this.lyricsTimer);
        break;
    }
    this.ref.detectChanges();
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
    this.playlistControl.next(false);
    this.ref.detectChanges();
  }
  playPausePlayer() {
    if (this.playerState === PlayerState.playing) {
      this.player.playerState = PlayerState.paused;
    } else {
      this.player.playerState = PlayerState.playing;
    }
  }
  //#endregion

  routingActivated(element: ElementRef) {
    this.hreflangTagHelper.setHreflangTags();

    // Login complete
    if (element instanceof LoginComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof TwoFactorComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof RegisterComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof SongShowComponent) {
      element.addToPlaylist.subscribe(this.addToPlaylist);
    } else if (element instanceof PlaylistShowComponent) {
      element.playbuttonClicked.subscribe((event: PlayButtonClickedEvent) => {
        switch (event.button) {
          case ePlaylistPlaybutton.addToQueue: {
            this.playlistControl.addToPlaylist(...event.songs);
          } break;
          case ePlaylistPlaybutton.playNow: {
            this.playlistControl.shuffle = false;
            this.playlistControl.setPlaylist(event.songs);
          } break;
          case ePlaylistPlaybutton.shuffle: {
            this.playlistControl.shuffle = true;
            this.playlistControl.setPlaylist(event.songs);
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
