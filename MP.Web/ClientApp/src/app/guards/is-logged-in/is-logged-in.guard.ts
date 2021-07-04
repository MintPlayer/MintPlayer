import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { AccountService } from '@mintplayer/ng-client';

@Injectable({
  providedIn: 'root'
})
export class IsLoggedInGuard implements CanActivate {
  constructor(
    private accountService: AccountService,
    private router: AdvancedRouter,
  ) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.accountService.currentUser().then((user) => {
      return true;
    }).catch((error: HttpErrorResponse) => {
      this.router.navigate(['/account','login'], {
        queryParams: {
          return: state.url
        }
      });
      return false;
    });
  }
}
