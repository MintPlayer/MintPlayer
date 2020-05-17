import { Injectable } from "@angular/core";
import { Promise } from 'q';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DailyMotionHelper {
  static scriptTag: HTMLScriptElement = null;

  public loadApi() {
    return Promise<boolean>((resolve, reject) => {
      if (typeof window !== 'undefined') {
        if (DailyMotionHelper.scriptTag === null) {
          let scriptTag = window.document.createElement('script');
          scriptTag.src = 'https://api.dmcdn.net/all.js';
          scriptTag.addEventListener('load', () => {
            console.log('DailyMotion player script loaded');
            this.apiReady.next(true);
            resolve(true);
          });
          scriptTag.addEventListener('error', () => {
            reject(new Error(`${DailyMotionHelper.scriptTag.src} failed to load`));
          });

          const firstScriptTag = window.document.getElementsByTagName('script')[0];
          firstScriptTag.parentNode.insertBefore(scriptTag, firstScriptTag);

          DailyMotionHelper.scriptTag = scriptTag;
        }
      }
    });
  }

  public unloadApi() {
    return Promise((resolve) => {
      if (typeof window !== 'undefined') {
        if (DailyMotionHelper.scriptTag !== null) {
          DailyMotionHelper.scriptTag.parentNode.removeChild(DailyMotionHelper.scriptTag);
          DailyMotionHelper.scriptTag = null;

          this.apiReady.next(false);
        }
      }
    });
  }

  public apiReady = new BehaviorSubject<boolean>(false);
}
