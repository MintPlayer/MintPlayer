import { Component, OnInit, Input, Output, EventEmitter, Inject } from '@angular/core';
import { LoginResult } from '../../../interfaces/loginResult';

@Component({
  selector: 'app-facebook-login',
  templateUrl: './facebook-login.component.html',
  styleUrls: ['./facebook-login.component.scss']
})
export class FacebookLoginComponent implements OnInit {

  private authWindow: Window;
  public isOpen: boolean = false;

  @Input() public action: "add" | "connect";

  @Output() public LoginSuccessOrFailed: EventEmitter<LoginResult> = new EventEmitter();

  launchFbLogin() {
    this.authWindow = window.open(`${this.baseUrl}/api/Account/${this.action}/Facebook`, null, 'width=600,height=400');
    this.isOpen = true;
    var timer = setInterval(() => {
      if (this.authWindow.closed) {
        this.isOpen = false;
        clearInterval(timer);
      }
    });
  }

  private listener: any;
  constructor(@Inject('BASE_URL') private baseUrl: string) {
    this.listener = this.handleMessage.bind(this);
    if (window.addEventListener) {
      window.addEventListener("message", this.listener, false);
    } else {
      (<any>window).attachEvent("onmessage", this.listener);
    }
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    if (window.removeEventListener) {
      window.removeEventListener("message", this.listener, false);
    } else {
      (<any>window).detachEvent("onmessage", this.listener);
    }
  }

  handleMessage(event: Event) {
    const message = event as MessageEvent;
    // Only trust messages from the below origin.
    if (!this.baseUrl.startsWith(message.origin)) return;

    // Filter out Augury
    if (message.data.messageSource != null)
      if (message.data.messageSource.indexOf("AUGURY_") > -1) return;
    // Filter out any other trash
    if (message.data == "" || message.data === null || message.data === undefined) return;

    debugger;
    const result = <LoginResult>JSON.parse(message.data);
    if (result.platform == "Facebook") {
      this.authWindow.close();
      this.LoginSuccessOrFailed.emit(result);
    }
  }
}
