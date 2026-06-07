import { DOCUMENT, inject, Injectable, RendererFactory2 } from '@angular/core';

/**
 * Minimal JSON-LD (schema.org structured data) helper. Upserts
 * <c>&lt;script type="application/ld+json"&gt;</c> tags in the document head, keyed by id so a
 * route change can replace a page's structured data rather than stacking duplicates.
 *
 * Dependency-free on purpose: <c>@mintplayer/ng-json-ld</c> is the intended package (D2) but its
 * latest release (18.0.0) peers on Angular ^18 and isn't Angular-22-ready yet — swap to it once
 * it's bumped. Uses Renderer2 + the injected DOCUMENT so it stays platform-safe (CSR today, Spark
 * prerender later).
 */
@Injectable({ providedIn: 'root' })
export class JsonLdService {
  private readonly document = inject(DOCUMENT);
  private readonly renderer = inject(RendererFactory2).createRenderer(null, null);

  /** Insert or replace the JSON-LD block identified by <paramref name="id"/>. */
  setSchema(id: string, schema: Record<string, unknown>): void {
    let script = this.document.getElementById(id) as HTMLScriptElement | null;
    if (!script) {
      script = this.renderer.createElement('script') as HTMLScriptElement;
      script.type = 'application/ld+json';
      script.id = id;
      this.renderer.appendChild(this.document.head, script);
    }
    script.textContent = JSON.stringify(schema);
  }

  /** Remove a previously-set JSON-LD block (e.g. when leaving a page). */
  removeSchema(id: string): void {
    const script = this.document.getElementById(id);
    if (script) {
      this.renderer.removeChild(this.document.head, script);
    }
  }
}
