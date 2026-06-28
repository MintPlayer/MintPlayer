import { trigger, state, style, transition, group, animate } from '@angular/animations';

export const SlideUpDownAnimation =
  trigger('slideUpDown', [
    state('in', style({
      'height': '*',
    })),
    state('out', style({
      'height': '0px',
    })),
    transition('in => out', [
      group([
        animate('600ms ease-in-out', style({
          'height': '0px'
        })),
      ])
    ]),
    transition('out => in', [
      group([
        animate('600ms ease-in-out', style({
          'height': '*'
        })),
      ])
    ]),
  ]);
