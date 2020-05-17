import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class PwaHelper {
  public isPwa() {
    return window.matchMedia('(display-mode: standalone)').matches;
  }
}
