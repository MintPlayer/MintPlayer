import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'wordCount'
})
export class WordCountPipe implements PipeTransform {

  transform(value: string) {
    if ((value === null) || (value === '')) {
      return 0;
    } else {
      return value
        .replace(/(^\s+)|(\s+$)/gi, '')
        .replace(/\s{2,}/gi, ' ')
        .split(' ').length;
    }
  }

}
