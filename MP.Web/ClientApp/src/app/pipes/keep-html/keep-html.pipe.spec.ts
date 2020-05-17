import { TestBed } from '@angular/core/testing';
import { KeepHtmlPipe } from './keep-html.pipe';

describe('KeepHtmlPipe', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('create an instance', () => {
    const pipe: KeepHtmlPipe = TestBed.get(KeepHtmlPipe);
    expect(pipe).toBeTruthy();
  });
});
