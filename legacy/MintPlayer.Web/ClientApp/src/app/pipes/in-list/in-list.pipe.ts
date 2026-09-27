import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'inList'
})
export class InListPipe implements PipeTransform {

  transform(items: any[], parameter: number) {
    return items.map(item => item.id).indexOf(parameter) > -1;
  }

}
