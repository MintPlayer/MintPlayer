import { Injectable } from "@angular/core";
import { Promise } from 'q';

@Injectable({
  providedIn: 'root'
})
export class LinkedinSdkHelper {
  static scriptTag: HTMLScriptElement = null;

  public loadSdk() {
    return Promise<boolean>((resolve, reject) => {
      if (LinkedinSdkHelper.scriptTag === null) {
        LinkedinSdkHelper.scriptTag = document.createElement('script');
        LinkedinSdkHelper.scriptTag.src = '//platform.linkedin.com/in.js';
        LinkedinSdkHelper.scriptTag.innerHTML = ' lang: en_US';

        const firstScriptTag = document.getElementsByTagName('script')[0];
        firstScriptTag.parentNode.insertBefore(LinkedinSdkHelper.scriptTag, firstScriptTag);

        resolve(true);
      }
    });
  }

  public unloadSdk() {
    return Promise((resolve) => {
      if (LinkedinSdkHelper.scriptTag !== null) {
        LinkedinSdkHelper.scriptTag.parentNode.removeChild(LinkedinSdkHelper.scriptTag);
        LinkedinSdkHelper.scriptTag = null;

        resolve(false);
      }
    });
  }
}
