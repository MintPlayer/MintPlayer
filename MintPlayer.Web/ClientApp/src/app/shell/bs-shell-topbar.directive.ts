import { Directive, TemplateRef, ViewContainerRef, Renderer2, OnInit, inject } from '@angular/core';

/**
 * Structural directive that projects its template into the <mp-shell> "topbar"
 * slot — the topbar counterpart to ng-bootstrap's `bsShellSidebar`. ng-bootstrap
 * 22 forbids host-binding `slot` on an <ng-template>, so we render the embedded
 * view and stamp `slot="topbar"` on its root element instead; the shell web
 * component then slots it into the topbar.
 *
 * TODO: promote this to @mintplayer/ng-bootstrap/shell once it ships there.
 */
@Directive({ selector: '[bsShellTopbar]' })
export class BsShellTopbarDirective implements OnInit {
  private readonly templateRef = inject(TemplateRef);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly renderer = inject(Renderer2);

  ngOnInit(): void {
    const view = this.viewContainer.createEmbeddedView(this.templateRef);
    for (const node of view.rootNodes) {
      if (node?.nodeType === 1) { // ELEMENT_NODE
        this.renderer.setAttribute(node, 'slot', 'topbar');
      }
    }
  }
}
