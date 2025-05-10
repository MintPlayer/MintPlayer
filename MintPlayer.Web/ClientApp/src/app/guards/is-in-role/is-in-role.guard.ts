import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { AccountService } from '@mintplayer/ng-client';
import { catchError, map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class IsInRoleGuard implements CanActivate {
  constructor(
    private accountService: AccountService,
    private router: AdvancedRouter,
  ) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.accountService.currentRoles().pipe(map((roles) => {
      let routeRoles = <string[]>next.data.roles;
      let intersect = roles.filter(r => routeRoles.indexOf(r) > -1);
      return intersect.length > 0;
    }), catchError((error: HttpErrorResponse) => {
      this.router.navigate(['/account', 'login'], {
        queryParams: {
          return: state.url
        }
      });
      return of(false);
    }));
  }
  
}
