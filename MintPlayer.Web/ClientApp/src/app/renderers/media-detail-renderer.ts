import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { EntityAttributeDefinition } from '@mintplayer/ng-spark/models';
import { SparkAttributeDetailRenderer } from '@mintplayer/ng-spark/renderers';
import { MediaPlayButton } from '../media/media-play-button';

/**
 * Detail-page view for a medium's URL: a play-triangle button (when playable) plus the URL as a link.
 * Bound via the "media-player" renderer on `Medium.Value`.
 */
@Component({
  selector: 'app-media-detail-renderer',
  imports: [MediaPlayButton],
  template: `
    <span class="d-inline-flex align-items-center gap-2">
      <app-media-play-button [url]="value()" />
      @if (value(); as url) {
        <a [href]="url" target="_blank" rel="noopener">{{ url }}</a>
      }
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MediaDetailRenderer implements SparkAttributeDetailRenderer {
  value = input<any>();
  attribute = input<EntityAttributeDefinition>();
  options = input<Record<string, any>>();
  formData = input<Record<string, any>>({});
}
