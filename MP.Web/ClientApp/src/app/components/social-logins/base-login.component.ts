import { Input, Output, EventEmitter, Inject, Directive } from '@angular/core';
import { API_VERSION, LoginResult } from '@mintplayer/ng-client';
import { PwaHelper } from '../../helpers/pwa.helper';

@Directive()
export class BaseLoginComponent {
  protected authWindow: Window;
  protected listener: any;
  public isOpen: boolean = false;

  @Input() public action: 'add' | 'connect';
  @Output() public LoginSuccessOrFailed: EventEmitter<LoginResult> = new EventEmitter();

  constructor(private externalUrl: string, private platform: string, private pwaHelper: PwaHelper, private apiVersion: string) {
    this.listener = this.handleMessage.bind(this);
    if (typeof window !== 'undefined') {
      if (window.addEventListener) {
        window.addEventListener('message', this.listener, false);
      } else {
        (<any>window).attachEvent('onmessage', this.listener);
      }
    }
  }

  protected dispose() {
    if (typeof window !== 'undefined') {
      if (window.removeEventListener) {
        window.removeEventListener('message', this.listener, false);
      } else {
        (<any>window).detachEvent('onmessage', this.listener);
      }
    }
  }

  showPopup() {
    if (typeof window !== 'undefined') {
      var medium = this.pwaHelper.isPwa() ? 'pwa' : 'web';

      this.authWindow = window.open(`${this.externalUrl}/web/${this.apiVersion}/Account/${this.action}/${medium}/${this.platform}?ngsw-bypass=true`, '_blank', 'width=600,height=400');

      this.isOpen = true;
      var timer = setInterval(() => {
        if (this.authWindow.closed) {
          this.isOpen = false;
          clearInterval(timer);
        }
      });
    }
  }

  handleMessage(event: Event) {
    const message = event as MessageEvent;

    // Only trust messages from the below origin.
    const messageOrigin = message.origin.replace(/^https?\:/, '');
    if (!this.externalUrl.startsWith(messageOrigin)) return;

    // Filter out Augury
    if (message.data.messageSource != null)
      if (message.data.messageSource.indexOf('AUGURY_') > -1) return;
    // Filter out any other trash
    if (message.data == '' || message.data == null) return;

    const result = <LoginResult>JSON.parse(message.data);
    //var medium = this.pwaHelper.isPwa() ? 'pwa' : 'web';
    if (result.platform === this.platform) {
      this.authWindow.close();
      this.LoginSuccessOrFailed.emit(result);
    }
  }
}
