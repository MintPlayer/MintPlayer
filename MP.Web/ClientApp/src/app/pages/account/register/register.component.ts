import { Component, OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../services/account/account.service';
import { UserData } from '../../../entities/user-data';
import { User } from '../../../entities/user';
import { Guid } from 'guid-typescript';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { Meta } from '@angular/platform-browser';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit, OnDestroy {
  constructor(
    private router: Router,
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
      pictureUrl: ''
    },
    password: '',
    passwordConfirmation: ''
  };

  public register() {
    this.accountService.register(this.data).then((result) => {
      this.accountService.login(this.data.user.email, this.data.password).then((login_result) => {
        if (login_result.status === true) {
          this.router.navigate(['/']);
          this.loginComplete.emit(login_result.user);
        } else {
          debugger;
        }
      }).catch((error) => {
        console.error('Could not register', error);
      });
    }).catch((error) => {
      console.error('Could not register', error);
    });
  }

  @Output() loginComplete: EventEmitter<User> = new EventEmitter();
}
