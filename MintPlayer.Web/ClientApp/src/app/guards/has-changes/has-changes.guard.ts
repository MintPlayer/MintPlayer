import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { HasChanges } from '../../interfaces/has-changes';
import { MyBeforeUnloadEvent } from '../../events/my-before-unload.event';

@Injectable({
  providedIn: 'root'
})
export class HasChangesGuard implements CanDeactivate<HasChanges> {

  canDeactivate(component: HasChanges, currentRoute: ActivatedRouteSnapshot, currentState: RouterStateSnapshot, nextState?: RouterStateSnapshot) {
    let ev = new MyBeforeUnloadEvent();
    if (component !== null) {
      component.beforeUnload(ev);
    }

    if (ev.defaultPrevented) {
      return false;
    } else {
      return true;
    }
  }
  
}
