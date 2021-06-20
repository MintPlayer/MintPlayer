import { DOCUMENT } from "@angular/common";
import { Inject, Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { AdvancedRouter } from "@mintplayer/ng-router";
import { HtmlLinkHelper } from "./html-link.helper";

@Injectable({
  providedIn: 'root'
})
export class HreflangTagHelper {
  constructor(
    private htmlLink: HtmlLinkHelper,
    private router: AdvancedRouter,
    @Inject(DOCUMENT) private document: HTMLDocument,
    //@Inject('BASE_URL') private baseUrl: string,
  ) { }

  private baseUrl = 'https://localhost:44329';

  public setHreflangTags() {
    this.document.querySelectorAll('link[rel="alternate"][hreflang]').forEach((link) => {
      link.parentNode.removeChild(link);
    });
    this.htmlLink.addTags([{
      rel: 'alternate',
      hreflang: 'en',
      href: this.baseUrl + this.router.serializeUrl(this.router.createUrlTree([], {
        queryParams: { lang: null },
        queryParamsHandling: ''
      }))
    }, {
      rel: 'alternate',
      hreflang: 'fr',
      href: this.baseUrl + this.router.serializeUrl(this.router.createUrlTree([], {
        queryParams: { lang: 'fr' },
        queryParamsHandling: ''
      }))
    }, {
      rel: 'alternate',
      hreflang: 'nl',
      href: this.baseUrl + this.router.serializeUrl(this.router.createUrlTree([], {
        queryParams: { lang: 'nl' },
        queryParamsHandling: ''
      }))
    }]);
  }
}
