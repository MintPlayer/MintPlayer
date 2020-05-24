import { Router, QueryParamsHandling, NavigationExtras, UrlTree } from '@angular/router';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ExtendedRouter {
  constructor(private router: Router) {
  }

  private _defaultQueryParamsHandling: QueryParamsHandling = null;
  public get defaultQueryParamsHandling() {
    return this._defaultQueryParamsHandling;
  }
  public set defaultQueryParamsHandling(value: QueryParamsHandling) {
    this._defaultQueryParamsHandling = value;
  }

  public navigate(commands: any[], extras?: NavigationExtras) {
    return this.router.navigate(commands, {
      queryParamsHandling: extras.queryParamsHandling ?? this.defaultQueryParamsHandling ?? '',
      fragment: extras.fragment,
      preserveFragment: extras.preserveFragment,
      queryParams: extras.queryParams,
      relativeTo: extras.relativeTo,
      replaceUrl: extras.replaceUrl,
      skipLocationChange: extras.skipLocationChange
    });
  }

  public navigateByUrl(url: string | UrlTree, extras?: NavigationExtras) {
    return this.router.navigateByUrl(url, {
      queryParamsHandling: extras.queryParamsHandling ?? this.defaultQueryParamsHandling ?? '',
      fragment: extras.fragment,
      preserveFragment: extras.preserveFragment,
      queryParams: extras.queryParams,
      relativeTo: extras.relativeTo,
      replaceUrl: extras.replaceUrl,
      skipLocationChange: extras.skipLocationChange
    });
  }

  public createUrlTree(commands: any[], extras?: NavigationExtras) {
    return this.router.createUrlTree(commands, extras);
  }

  public serializeUrl(url: UrlTree) {
    return this.router.serializeUrl(url);
  }
}
