import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { BaseLoginComponent } from '../base-login-component';

@Component({
  selector: 'app-github-login',
  templateUrl: './github-login.component.html',
  styleUrls: ['./github-login.component.scss']
})
export class GithubLoginComponent extends BaseLoginComponent implements OnInit, OnDestroy {

  constructor(@Inject('BASE_URL') protected baseUrl: string) {
    super(baseUrl, "GitHub");
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.dispose();
  }
}
