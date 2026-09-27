import { TranslateLoader } from '@ngx-translate/core';
import { of } from 'rxjs';

import * as translationEn from '../../assets/i18n/en.json';
import * as translationNl from '../../assets/i18n/nl.json';

export class TranslateJsonLoader implements TranslateLoader {
  constructor() {
  }

  public getTranslation(lang: string) {
    switch (lang) {
      case 'nl': {
        return of(translationNl);
      } break;
      default: {
        return of(translationEn);
      } break;
    }
  }
}
