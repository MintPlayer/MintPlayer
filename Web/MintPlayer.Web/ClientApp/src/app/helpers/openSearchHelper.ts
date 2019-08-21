import { Injectable } from "@angular/core";
import { Promise } from 'q';

@Injectable({
  providedIn: 'root'
})
export class OpenSearchHelper {
  public addOpenSearch(title: string, osdxUrl: string) {
    return Promise((resolve) => {

      if (typeof document !== 'undefined') {
        const tag = document.createElement('link');
        tag.rel = 'search';
        tag.type = 'application/opensearchdescription+xml';
        tag.title = title;
        tag.href = osdxUrl;

        document.head.appendChild(tag);
      }

    })
  }
}
