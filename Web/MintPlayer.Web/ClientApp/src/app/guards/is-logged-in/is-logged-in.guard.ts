import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AccountService } from '../../services/account/account.service';

@Injectable({
  providedIn: 'root'
})
export class IsLoggedInGuard implements CanActivate {
  constructor(private accountService: AccountService, private router: Router) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.accountService.currentUser().toPromise().then((user) => {
      return true;
    }).catch((error: HttpErrorResponse) => {
      this.router.navigate(['/login'], {
        queryParams: {
          return: state.url
        }
      });
      return false;
    });
  }
}
