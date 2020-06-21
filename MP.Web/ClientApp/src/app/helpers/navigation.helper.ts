import { Injectable } from '@angular/core';
import { Router, NavigationExtras, UrlTree, ActivatedRoute } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class NavigationHelper {
  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {
  }

  public createUrlTree(commands: any[], navigationExtras?: NavigationExtras) {
    return this.router.createUrlTree(commands, navigationExtras);
  }

  public serializeUrl(url: UrlTree) {
    return this.router.serializeUrl(url);
  }

  public navigate(commands: any[], extras?: NavigationExtras) {
    return this.router.navigate(commands, extras);
  }

  public navigateByUrl(url: string | UrlTree, extras?: NavigationExtras) {
    return this.router.navigateByUrl(url, extras);
  }
}
