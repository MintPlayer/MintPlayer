import { Pipe, PipeTransform } from '@angular/core';
import { SlugifyHelper } from '../../helpers/slugify.helper';

@Pipe({
  name: 'slugify'
})
export class SlugifyPipe implements PipeTransform {
  constructor(private slugifyHelper: SlugifyHelper) {
  }

  transform(value: any) {
    return this.slugifyHelper.slugify(value);
  }

}
