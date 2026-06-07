import { inject, Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';

/**
 * Site-wide document-title policy. A route's <c>title</c> is suffixed with the site name
 * ("Medium types | MintPlayer"); routes without a title fall back to the bare site name.
 * Registered via <c>{ provide: TitleStrategy, useClass: MintPlayerTitleStrategy }</c>.
 */
@Injectable({ providedIn: 'root' })
export class MintPlayerTitleStrategy extends TitleStrategy {
  static readonly siteName = 'MintPlayer';
  private readonly title = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const pageTitle = this.buildTitle(snapshot);
    this.title.setTitle(
      pageTitle ? `${pageTitle} | ${MintPlayerTitleStrategy.siteName}` : MintPlayerTitleStrategy.siteName,
    );
  }
}
