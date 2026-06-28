import { ChangeDetectionStrategy, Component, effect, inject, input } from '@angular/core';
import { PoContextService } from './po-context.service';

/**
 * Renders nothing — it exists only to forward the PO detail page's resolved item + entity type (handed in
 * via the framework's `extraContentTemplate` context) into {@link PoContextService}. Keeping the write in
 * a component effect (rather than a side-effect inside a template binding) keeps it out of change-detection
 * and gives a single, obvious place where the context is published.
 */
@Component({
  selector: 'app-po-context-capture',
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PoContextCapture {
  private readonly context = inject(PoContextService);

  /** The framework's loaded PersistentObject (`{ id, breadcrumb, attributes, … }`). */
  readonly item = input<{ id?: string } | null>();
  /** The framework's resolved entity type (`{ clrType, alias, id, … }`). */
  readonly entityType = input<{ clrType?: string; alias?: string; id?: string } | null>();

  constructor() {
    effect(() => {
      const item = this.item();
      const et = this.entityType();
      if (item?.id && et?.clrType) {
        this.context.set({ id: item.id, clrType: et.clrType, routeType: et.alias || et.id || et.clrType });
      }
    });
  }
}
