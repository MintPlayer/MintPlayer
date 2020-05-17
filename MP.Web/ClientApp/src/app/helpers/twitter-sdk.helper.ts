import { Injectable } from "@angular/core";
import { Promise } from 'q';

@Injectable({
  providedIn: 'root'
})
export class TwitterSdkHelper {
  static scriptTag: HTMLScriptElement = null;

  public loadSdk() {
    return Promise<boolean>((resolve, reject) => {
      if (TwitterSdkHelper.scriptTag === null) {
        TwitterSdkHelper.scriptTag = document.createElement('script');
        TwitterSdkHelper.scriptTag.id = 'twitter-wjs';
        TwitterSdkHelper.scriptTag.src = 'https://platform.twitter.com/widgets.js';

        const firstScriptTag = document.getElementsByTagName('script')[0];
        firstScriptTag.parentNode.insertBefore(TwitterSdkHelper.scriptTag, firstScriptTag);

        resolve(true);
      }
    });
  }

  public unloadSdk() {
    return Promise((resolve) => {
      if (TwitterSdkHelper.scriptTag !== null) {
        TwitterSdkHelper.scriptTag.parentNode.removeChild(TwitterSdkHelper.scriptTag);
        TwitterSdkHelper.scriptTag = null;

        resolve(false);
      }
    });
  }
}
