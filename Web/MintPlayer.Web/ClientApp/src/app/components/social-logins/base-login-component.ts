import { Input, Output, EventEmitter } from '@angular/core';
import { LoginResult } from '../../interfaces/loginResult';

export class BaseLoginComponent {
  protected authWindow: Window;
  protected listener: any;
  public isOpen: boolean = false;

  @Input() public action: "add" | "connect";
  @Output() public LoginSuccessOrFailed: EventEmitter<LoginResult> = new EventEmitter();

  constructor(protected baseUrl: string, protected platform: string) {
    this.listener = this.handleMessage.bind(this);
    if (window.addEventListener) {
      window.addEventListener("message", this.listener, false);
    } else {
      (<any>window).attachEvent("onmessage", this.listener);
    }
  }

  protected dispose() {
    if (window.removeEventListener) {
      window.removeEventListener("message", this.listener, false);
    } else {
      (<any>window).detachEvent("onmessage", this.listener);
    }
  }

  showPopup() {
    debugger;
    this.authWindow = window.open(`${this.baseUrl}/api/Account/${this.action}/${this.platform}`, null, 'width=600,height=400');
    this.isOpen = true;
    var timer = setInterval(() => {
      if (this.authWindow.closed) {
        this.isOpen = false;
        clearInterval(timer);
      }
    });
  }

  private handleMessage(event: Event) {
    const message = event as MessageEvent;
    // Only trust messages from the below origin.
    if (!this.baseUrl.startsWith(message.origin)) return;

    // Filter out Augury
    if (message.data.messageSource != null)
      if (message.data.messageSource.indexOf("AUGURY_") > -1) return;
    // Filter out any other trash
    if (message.data == "" || message.data === null || message.data === undefined) return;
    // Filter out Webpack
    if (message.data.type !== undefined) return;

    console.log(message.data.type);

    debugger;
    const result = <LoginResult>JSON.parse(message.data);
    if (result.platform == this.platform) {
      this.authWindow.close();
      this.LoginSuccessOrFailed.emit(result);
    }
  }
}
