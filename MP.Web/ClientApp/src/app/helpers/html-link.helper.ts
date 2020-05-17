import { Injectable, Inject } from "@angular/core";
import { DOCUMENT } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class HtmlLinkHelper {
  constructor(@Inject(DOCUMENT) private document: HTMLDocument, @Inject('BASE_URL') private baseUrl: string) {
  }

  private linkTags = {};

  public set(key: string, url: string) {
    let existingLinkTag = this.linkTags[key];
    if (existingLinkTag === undefined) {
      this.linkTags[key] = existingLinkTag = this.document.createElement('link');
      this.document.head.appendChild(existingLinkTag);
    }

    existingLinkTag.setAttribute('rel', key);
    if (url.startsWith(this.baseUrl)) {
      existingLinkTag.setAttribute('href', url);
    } else {
      existingLinkTag.setAttribute('href', this.baseUrl + url);
    }
  }

  public setCanonicalWithoutQuery() {
    let loc = this.document.location;
    let noQueryUrl = loc.href.split('?')[0];
    this.set('canonical', noQueryUrl);
  }

  public unset(key: string) {
    let existingLinkTag = this.linkTags[key];
    if (existingLinkTag !== undefined) {
      existingLinkTag.parentNode.removeChild(existingLinkTag);
      this.linkTags[key] = undefined;
    }
  }
}
