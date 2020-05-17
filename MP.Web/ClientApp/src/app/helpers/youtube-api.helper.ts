import { Injectable } from '@angular/core';
import { Promise } from 'q';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class YoutubeHelper {
  static scriptTag: HTMLScriptElement = null;

  public loadApi() {
    return Promise<boolean>((resolve, reject) => {
      if (typeof window !== 'undefined') {
        if (YoutubeHelper.scriptTag === null) {
          window['onYouTubeIframeAPIReady'] = () => {
            this.apiReady.next(true);
            resolve(true);
          };
          //console.log('Assigned window.onYouTubeIframeAPIReady');

          YoutubeHelper.scriptTag = window.document.createElement('script');
          YoutubeHelper.scriptTag.src = 'https://www.youtube.com/iframe_api';

          const firstScriptTag = window.document.getElementsByTagName('script')[0];
          firstScriptTag.parentNode.insertBefore(YoutubeHelper.scriptTag, firstScriptTag);
          //console.log('Inserted YouTube iframe api');
        }
      }
    });
  }

  public unloadApi() {
    return Promise((resolve) => {
      if (typeof window !== 'undefined') {
        if (YoutubeHelper.scriptTag !== null) {
          YoutubeHelper.scriptTag.parentNode.removeChild(YoutubeHelper.scriptTag);
          YoutubeHelper.scriptTag = null;
          //console.log('Removed YouTube iframe api');

          this.apiReady.next(false);
        }
      }
    });
  }

  public apiReady = new BehaviorSubject<boolean>(
    (typeof window === 'undefined')
      ? false
      : window['YT'] !== undefined
  );
}
