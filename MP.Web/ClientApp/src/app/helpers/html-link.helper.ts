import { Injectable, Inject } from "@angular/core";
import { DOCUMENT } from '@angular/common';
import { BASE_URL } from "@mintplayer/ng-base-url";

@Injectable({
  providedIn: 'root'
})
export class HtmlLinkHelper {
  constructor(@Inject(DOCUMENT) private document: HTMLDocument, @Inject(BASE_URL) private baseUrl: string) {
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




  public addTags(tags: LinkDefinition[]) {
    let createdTags = tags.map((tag) => {
      let link = this.document.createElement('link');
      link.rel = tag.rel;
      link.href = tag.href;
      link.hreflang = tag.hreflang;

      this.document.head.appendChild(link);
      return link;
    });

    return createdTags;
  }

  public removeTags(tags: HTMLLinkElement[]) {
    tags.forEach((tag) => {
      tag.parentNode.removeChild(tag);
    });
  }
}

export class LinkDefinition {
  rel: string;
  href: string;
  hreflang: string;
}
