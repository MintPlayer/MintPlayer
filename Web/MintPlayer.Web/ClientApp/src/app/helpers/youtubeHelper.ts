import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Promise } from 'q';

@Injectable({
  providedIn: 'root'
})
export class YoutubeHelper {
  //public loadApi() {
  //  return Promise((resolve) => {

  //    window['onYouTubeIframeAPIReady'] = resolve;

  //    const tag = window.document.createElement('script');
  //    tag.src = 'https://www.youtube.com/iframe_api';

  //    const firstScriptTag = window.document.getElementsByTagName('script')[0];
  //    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

  //  });
  //}

  public apiReady = new BehaviorSubject<boolean>((() => {
    if (typeof window !== 'undefined') {
      return window['YT'] !== undefined;
    } else {
      return false;
    }
  })());
}
