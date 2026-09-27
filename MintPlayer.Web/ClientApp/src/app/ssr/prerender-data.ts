import { InjectionToken } from '@angular/core';

/**
 * The request-scoped data ASP.NET Core supplied for this render (MintPlayerSpaPrerenderingService
 * .OnSupplyData → params.data, camelCased). Provided only by main.server.ts; `null` in the browser.
 */
export const PRERENDER_DATA = new InjectionToken<Record<string, unknown> | null>('PRERENDER_DATA', {
  providedIn: 'root',
  factory: () => null,
});
