import { Injectable } from '@angular/core';
import { EnumItem } from '../entities/enum-item';

@Injectable({
  providedIn: 'root'
})
export class EnumHelper {
  public getItems(tenum: any) {
    //var enumValues = Object.values(TEnum);
    var enumValues = Object.values(tenum);
    var enumItems = enumValues.slice(enumValues.length / 2).map<EnumItem>((id) => {
      var id_num = <number>id;
      return { id: id_num, description: <string>enumValues[id_num] };
    });
    return enumItems;
  }
}
