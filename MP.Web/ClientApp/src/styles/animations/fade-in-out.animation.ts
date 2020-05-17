import { trigger, state, style, transition, group, animate } from '@angular/animations';

export const FadeInOutAnimation =
  trigger('fadeInOut', [
    state('in', style({
      'opacity': '1',
    })),
    state('out', style({
      'opacity': '0',
    })),
    transition('in => out', [
      group([
        animate('600ms ease-in-out', style({
          'opacity': '0'
        })),
      ])
    ]),
    transition('out => in', [
      group([
        animate('600ms ease-in-out', style({
          'opacity': '1'
        })),
      ])
    ]),
  ]);
