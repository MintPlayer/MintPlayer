import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'propVal'
})
export class PropValPipe implements PipeTransform {

  transform(value: any, path: string): any {
    return path.split('.').reduce((prev, curr) => {
      return prev ? prev[curr] : null
    }, value || self);
  }

}
