import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { BaseLoginComponent } from '../base-login-component';

@Component({
  selector: 'app-twitter-login',
  templateUrl: './twitter-login.component.html',
  styleUrls: ['./twitter-login.component.scss']
})
export class TwitterLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {

  constructor(@Inject('BASE_URL') baseUrl: string) {
    super(baseUrl, "Twitter");
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }

}

