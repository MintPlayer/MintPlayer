import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { EntityAttributeDefinition } from '@mintplayer/ng-spark/models';
import { SparkAttributeColumnRenderer } from '@mintplayer/ng-spark/renderers';

/** Datatable cell: a small colour swatch for a hex colour value. */
@Component({
  selector: 'app-color-column-renderer',
  template: `
    @if (value(); as colorVal) {
      <span class="d-inline-block align-middle border rounded"
            [style.background-color]="colorVal"
            style="width: 1.5em; height: 1.5em;"
            [title]="colorVal"></span>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorColumnRenderer implements SparkAttributeColumnRenderer {
  value = input<any>();
  attribute = input<EntityAttributeDefinition>();
  options = input<Record<string, any>>();
}
