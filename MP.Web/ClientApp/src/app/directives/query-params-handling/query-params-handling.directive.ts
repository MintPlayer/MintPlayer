import { Directive } from '@angular/core';
import { RouterLinkWithHref, QueryParamsHandling } from '@angular/router';

@Directive({
  selector: 'a[routerLink]'
})
export class QueryParamsHandlingDirective extends RouterLinkWithHref {
  queryParamsHandling: QueryParamsHandling = 'merge';
}
