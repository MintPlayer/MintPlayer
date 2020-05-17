import { Injectable } from "@angular/core";
import { Promise } from 'q';

@Injectable({
  providedIn: 'root'
})
export class FacebookSdkHelper {
  static scriptTag: HTMLScriptElement = null;

  public loadSdk() {
    return Promise<boolean>((resolve, reject) => {
      if (FacebookSdkHelper.scriptTag === null) {
        FacebookSdkHelper.scriptTag = document.createElement('script');
        FacebookSdkHelper.scriptTag.id = 'facebook-jssdk';
        FacebookSdkHelper.scriptTag.src = 'https://connect.facebook.net/en_US/sdk.js#xfbml=1&version=v3.0';

        const firstScriptTag = document.getElementsByTagName('script')[0];
        firstScriptTag.parentNode.insertBefore(FacebookSdkHelper.scriptTag, firstScriptTag);

        resolve(true);
      }
    });
  }

  public unloadSdk() {
    return Promise((resolve) => {
      if (FacebookSdkHelper.scriptTag !== null) {
        FacebookSdkHelper.scriptTag.parentNode.removeChild(FacebookSdkHelper.scriptTag);
        FacebookSdkHelper.scriptTag = null;

        resolve(false);
      }
    });
  }
}
