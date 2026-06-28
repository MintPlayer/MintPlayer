import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'linify'
})
export class LinifyPipe implements PipeTransform {

  transform(value: string) {
    return value.split('\n').filter((line) => line !== '');
  }

}
