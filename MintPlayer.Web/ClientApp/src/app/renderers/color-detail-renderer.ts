import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { EntityAttributeDefinition } from '@mintplayer/ng-spark/models';
import { SparkAttributeDetailRenderer } from '@mintplayer/ng-spark/renderers';

/** PO detail page: colour swatch + hex value. */
@Component({
  selector: 'app-color-detail-renderer',
  template: `
    @if (value(); as colorVal) {
      <span class="d-inline-block align-middle border rounded me-2"
            [style.background-color]="colorVal"
            style="width: 1.5em; height: 1.5em;"></span>
      <code>{{ colorVal }}</code>
    } @else {
      -
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorDetailRenderer implements SparkAttributeDetailRenderer {
  value = input<any>();
  attribute = input<EntityAttributeDefinition>();
  options = input<Record<string, any>>();
  formData = input<Record<string, any>>({});
}
