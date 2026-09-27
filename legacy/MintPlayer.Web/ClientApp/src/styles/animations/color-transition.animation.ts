import { trigger, state, style, transition, group, animate } from '@angular/animations';

export const ColorTransitionAnimation =
  trigger('colorTransition', [
    state('color1', style({
      'background': "{{ color1 }}",
    }), {
      params: {
        color1: '#000',
      }
    }),
    state('color2', style({
      'background': "{{ color2 }}",
    }), {
      params: {
        color2: '#444',
      }
    }),
    transition('color1 => color2', [
      group([
        animate('{{ duration }} ease-in-out', style({
          'background': "{{ color2 }}"
        })),
      ])
    ], {
      params: {
        color1: '#000',
        color2: '#444',
        duration: '1s',
      }
    }),
    transition('color2 => color1', [
      group([
        animate('{{ duration }} ease-in-out', style({
          'background': "{{ color1 }}"
        })),
      ])
    ], {
      params: {
        color1: '#000',
        color2: '#444',
        duration: '1s',
      }
    }),
  ]);
