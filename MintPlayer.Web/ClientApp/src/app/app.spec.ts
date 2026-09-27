import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should emit site-wide JSON-LD structured data', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    // @mintplayer/ng-seo's [jsonLd] directive renders each schema as a
    // <script type="application/ld+json"> appended to document.head.
    const scripts = Array.from(document.head.querySelectorAll('script[type="application/ld+json"]'));
    const payloads = scripts.map((s) => s.textContent ?? '');
    expect(payloads.some((p) => p.includes('"WebSite"') && p.includes('MintPlayer'))).toBe(true);
    expect(payloads.some((p) => p.includes('"Organization"') && p.includes('MintPlayer'))).toBe(true);
    fixture.destroy(); // the directive removes its <script> tags on destroy
  });
});
