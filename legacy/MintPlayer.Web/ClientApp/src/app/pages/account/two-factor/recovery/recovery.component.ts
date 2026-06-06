import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { AccountService, MINTPLAYER_API_VERSION, User } from '@mintplayer/ng-client';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-recovery',
  templateUrl: './recovery.component.html',
  styleUrls: ['./recovery.component.scss']
})
export class RecoveryComponent implements OnInit {

  constructor(
    private httpClient: HttpClient,
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
    private accountService: AccountService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
  ) {
  }

  baseUrl = this.baseUrlService.getBaseUrl();
  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.returnUrl = params['return'] || '/';
    });
  }

  recoveryCode: string = '';
  isError: boolean = false;
  private returnUrl: string = '';

  recoverAccount() {
    this.httpClient.post(`${this.baseUrl}/web/${this.apiVersion}/Account/two-factor-recovery`, { code: this.recoveryCode }).subscribe({
      next: () => {
        this.accountService.csrfRefresh().subscribe({
          next: () => {

            this.accountService.currentUser().subscribe({
              next: (user) => {
                this.loginComplete.next(user);
                this.router.navigateByUrl(this.returnUrl);
              }
            });

          },
          error: (error) => {
            this.isError = true;
          }
        })
      },
      error: (error) => {
        this.isError = true;
      }
    });
  }

  loginComplete: Subject<User> = new Subject<User>();
}
