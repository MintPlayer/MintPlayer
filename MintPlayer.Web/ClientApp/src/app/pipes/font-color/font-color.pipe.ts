import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fontColor'
})
export class FontColorPipe implements PipeTransform {

  transform(color_hex: any): any {
    if (color_hex === undefined || color_hex === null || color_hex.length < 7 || typeof (color_hex) !== 'string') {
      return '#FFFFFF';
    }
    const R_HEX: string = color_hex.substr(1, 2);
    const G_HEX: string = color_hex.substr(3, 2);
    const B_HEX: string = color_hex.substr(5, 2);
    const R_DEC: number = parseInt(R_HEX, 16);
    const G_DEC: number = parseInt(G_HEX, 16);
    const B_DEC: number = parseInt(B_HEX, 16);
    const CONTRAST_HEX: string = R_DEC * 0.299 + G_DEC * 0.587 + B_DEC * 0.114 > 186 ? '#000000' : '#FFFFFF';
    return CONTRAST_HEX;
  }

}
