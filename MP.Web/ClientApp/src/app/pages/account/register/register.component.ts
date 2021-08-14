import { Component, OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { AccountService, User, UserData } from '@mintplayer/ng-client';
import { LoginStatus } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { Guid } from 'guid-typescript';
import { Subject } from 'rxjs';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit, OnDestroy {
  constructor(
    private router: AdvancedRouter,
    private accountService: AccountService,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta
  ) {
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: 'Here you can create your MintPlayer account. Start building your playlists now.'
    }]);
  }
  private addOpenGraphTags() {

  }
  private addTwitterCard() {

  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion

  public data: UserData = {
    user: {
      id: Guid.createEmpty()['value'],
      userName: '',
      email: '',
      isTwoFactorEnabled: false,
      bypass2faForExternalLogin: false,
      pictureUrl: ''
    },
    password: '',
    passwordConfirmation: ''
  };

  public register() {
    this.accountService.register(this.data).then((result) => {
      this.accountService.login(this.data.user.email, this.data.password).then((login_result) => {
        this.accountService.csrfRefresh().then(() => {
          if (login_result.status === LoginStatus.success) {
            this.router.navigate(['/']);
            this.loginComplete.next(login_result.user);
          } else {
            debugger;
          }
        });
      }).catch((error) => {
        console.error('Could not register', error);
      });
    }).catch((error) => {
      console.error('Could not register', error);
    });
  }

  @Output() loginComplete: Subject<User> = new Subject<User>();
}
