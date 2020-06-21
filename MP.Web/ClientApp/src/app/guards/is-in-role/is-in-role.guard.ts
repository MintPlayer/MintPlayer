import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { AccountService } from '../../services/account/account.service';
import { HttpErrorResponse } from '@angular/common/http';
import { NavigationHelper } from '../../helpers/navigation.helper';

@Injectable({
  providedIn: 'root'
})
export class IsInRoleGuard implements CanActivate {
  constructor(
    private accountService: AccountService,
    private navigation: NavigationHelper,
  ) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.accountService.currentRoles().then((roles) => {
      let routeRoles = <string[]>next.data.roles;
      let intersect = roles.filter(r => routeRoles.indexOf(r) > -1);
      return intersect.length > 0;
    }).catch((error: HttpErrorResponse) => {
      this.navigation.navigate(['/account', 'login'], {
        queryParams: {
          return: state.url
        }
      });
      return false;
    });
  }
  
}
