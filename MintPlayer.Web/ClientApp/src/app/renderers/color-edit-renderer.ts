import { ChangeDetectionStrategy, Component, effect, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsColorPickerComponent } from '@mintplayer/ng-bootstrap/color-picker';
import { EntityAttributeDefinition } from '@mintplayer/ng-spark/models';
import { SparkAttributeEditRenderer } from '@mintplayer/ng-spark/renderers';

/** Create/edit form: ng-bootstrap colour picker bound to a hex value. */
@Component({
  selector: 'app-color-edit-renderer',
  imports: [FormsModule, BsColorPickerComponent],
  template: `
    <div class="d-flex align-items-start gap-3">
      <bs-color-picker
        [size]="options()?.['size'] ?? 150"
        [ngModel]="currentColor()"
        (ngModelChange)="onColorChange($event)">
      </bs-color-picker>
      @if (currentColor(); as c) {
        <div class="d-flex align-items-center gap-2 mt-2">
          <span class="d-inline-block border rounded"
                [style.background-color]="c"
                style="width: 2em; height: 2em;"></span>
          <code>{{ c }}</code>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorEditRenderer implements SparkAttributeEditRenderer {
  value = input<any>();
  attribute = input<EntityAttributeDefinition>();
  options = input<Record<string, any>>();
  valueChange = input<(value: any) => void>(() => {});

  currentColor = signal<string>('#000000');

  constructor() {
    effect(() => {
      const v = this.value();
      if (v) {
        this.currentColor.set(v);
      }
    });
  }

  onColorChange(hex: string): void {
    this.currentColor.set(hex);
    this.valueChange()?.(hex);
  }
}
