import { Component, ElementRef } from '@angular/core';
import { eToggleButtonState } from './enums/eToggleButtonState';
import { eSidebarState } from './enums/eSidebarState';
import { User } from './interfaces/account/user';
import { LoginComponent } from './pages/account/login/login.component';
import { RegisterComponent } from './pages/account/register/register.component';
import { ShowComponent as SongShowComponent } from './pages/song/show/show.component';
import { AccountService } from './services/account/account.service';
import { YoutubeHelper } from './helpers/youtubeHelper';
import { Song } from './interfaces/song';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'ClientApp';
  activeUser: User = null;
  fullWidth: boolean = false;
  toggleButtonState: eToggleButtonState = eToggleButtonState.auto;
  sidebarState: eSidebarState = eSidebarState.auto;
  playlistToggleButtonState: eToggleButtonState = eToggleButtonState.closed;

  constructor(private accountService: AccountService, private youtubeHelper: YoutubeHelper) {
    this.accountService.currentUser().subscribe((user) => {
      this.activeUser = user;
    }, (error) => {
      this.activeUser = null;
    });
    this.youtubeHelper.loadApi().then(() => {
      console.log('loaded youtube api');
      this.youtubeHelper.apiReady.next(true);
    });
  }

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
    this.accountService.logout().subscribe(() => {
      this.activeUser = null;
      localStorage.removeItem('auth_token');
    });
  }

  //#region Playlist
  playlist: Song[] = [];
  currentSong: Song = null;
  addToPlaylist = (song: Song) => {
    this.playlist.push(song);
  }
  //#endregion

  routingActivated(element: ElementRef) {
    // Login complete
    if (element instanceof LoginComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof RegisterComponent) {
      element.loginComplete.subscribe(this.loginCompleted);
    } else if (element instanceof SongShowComponent) {
      element.addToPlaylist.subscribe(this.addToPlaylist);
    }
  }

  routingDeactivated(element: ElementRef) {
    // Login complete
    if (element instanceof LoginComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof RegisterComponent) {
      element.loginComplete.unsubscribe();
    } else if (element instanceof SongShowComponent) {
      element.addToPlaylist.unsubscribe();
    }
  }
}
