import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { EntityAttributeDefinition } from '@mintplayer/ng-spark/models';
import { SparkAttributeColumnRenderer } from '@mintplayer/ng-spark/renderers';
import { MediaPlayButton } from '../media/media-play-button';

/**
 * Datatable cell for a medium's URL (the leading column of the Media grid): a play-triangle button when
 * the URL is playable by the <video-player>, followed by the URL text. Bound via the "media-player"
 * renderer on `Medium.Value`.
 */
@Component({
  selector: 'app-media-column-renderer',
  imports: [MediaPlayButton],
  template: `
    <span class="d-inline-flex align-items-center gap-2">
      <app-media-play-button [url]="value()" />
      <span class="text-truncate" style="max-width: 28em;" [title]="value()">{{ value() }}</span>
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MediaColumnRenderer implements SparkAttributeColumnRenderer {
  value = input<any>();
  attribute = input<EntityAttributeDefinition>();
  options = input<Record<string, any>>();
}
