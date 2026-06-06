import { SlugifyPipe } from './slugify.pipe';

describe('SlugifyPipe', () => {
  it('create an instance', () => {
    const pipe = SlugifyPipe.prototype;
    expect(pipe).toBeTruthy();
  });
});
