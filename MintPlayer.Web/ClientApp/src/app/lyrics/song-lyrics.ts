import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { PlayerService } from '../player/player.service';
import { LyricsService, LyricsTiming, SongLyrics as SongLyricsData } from './lyrics.service';

/**
 * Lyrics panel on a song's detail page (mounted by {@link AppPoDetail}). The lyrics *text* is read-only here
 * — it's curated in the Spark song edit form (a MultiLineString field) like the rest of the catalog. This
 * panel does two things:
 *
 *  - **Display** — shows the lyrics, highlighting the active line when *this* song is the one playing. The
 *    highlight uses the timing for the currently-playing medium URL (timing is per recording) and the live
 *    {@link PlayerService.progress}: the latest line whose start has passed.
 *  - **Sync timing** (Editor / Administrator, signalled by `canEdit`) — while this song plays, a per-line
 *    "Set" button stamps the current playback time onto that line. Saving writes only the timing.
 *
 * Syncing requires the song to be playing — that's the recording whose timing is being captured.
 */
@Component({
  selector: 'app-song-lyrics',
  template: `
    <section class="card my-3">
      <div class="card-header d-flex align-items-center gap-2">
        <i class="bi bi-card-text"></i>
        <span class="fw-semibold flex-grow-1">Lyrics</span>
        @if (data()?.canEdit) {
          @if (mode() === 'view') {
            <button type="button" class="btn btn-sm btn-outline-primary" (click)="startSync()">
              <i class="bi bi-stopwatch"></i> Sync timing
            </button>
          } @else {
            <button type="button" class="btn btn-sm btn-primary" (click)="save()" [disabled]="saving()">Save</button>
            <button type="button" class="btn btn-sm btn-outline-secondary" (click)="cancel()" [disabled]="saving()">Cancel</button>
          }
        }
      </div>
      <div class="card-body">
        @if (lines().length && lines()[0]) {
          @if (mode() === 'view') {
            <div class="lyrics-view">
              @for (line of lines(); track $index) {
                <div [class.fw-bold]="$index === activeLineIndex()" [class.text-primary]="$index === activeLineIndex()">
                  {{ line || ' ' }}
                </div>
              }
            </div>
          } @else if (syncUrl()) {
            <p class="small text-secondary mb-1">
              Syncing to the playing recording — press <em>Set</em> on a line as it starts.
            </p>
            <ol class="lyrics-sync list-unstyled mb-0">
              @for (line of lines(); track $index) {
                <li class="d-flex align-items-center gap-2 py-1">
                  <button type="button" class="btn btn-sm btn-outline-secondary" (click)="markStart($index)">Set</button>
                  <span class="text-secondary small" style="width: 4em;">{{ formatTime(startTimeForLine($index)) }}</span>
                  <span class="flex-grow-1 text-truncate" [class.fw-bold]="$index === activeLineIndex()">{{ line || ' ' }}</span>
                </li>
              }
            </ol>
          } @else {
            <p class="small text-secondary mb-0">Play this song to capture line timings.</p>
          }
        } @else {
          <p class="text-secondary mb-0">No lyrics yet.</p>
        }
      </div>
    </section>
  `,
  styles: [`.lyrics-view { white-space: pre-wrap; } .lyrics-view > div { min-height: 1.4em; }`],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SongLyrics {
  private readonly player = inject(PlayerService);
  private readonly service = inject(LyricsService);

  /** The song's id (e.g. `"Songs/6"`). */
  readonly songId = input.required<string>();

  protected readonly data = signal<SongLyricsData | null>(null);
  protected readonly mode = signal<'view' | 'sync'>('view');
  protected readonly saving = signal(false);

  /** Working copy of the timings while in sync mode. */
  private readonly editTimings = signal<LyricsTiming[]>([]);

  constructor() {
    effect(() => {
      const id = this.songId();
      this.mode.set('view');
      this.data.set(null);
      this.service.get(id).then((result) => this.data.set(result)).catch(() => {});
    });
  }

  /** Lyrics text split into display lines. */
  protected readonly lines = computed(() => (this.data()?.text ?? '').split('\n'));

  /** The URL of the medium currently playing, but only when it's *this* song (else syncing is N/A). */
  protected readonly syncUrl = computed(() => {
    const entry = this.player.currentEntry();
    return entry && entry.key === this.songId() ? entry.url : null;
  });

  /** Active line index: saved timings in view mode, the working copy while syncing; -1 when N/A. */
  protected readonly activeLineIndex = computed(() => {
    const url = this.syncUrl();
    if (!url) {
      return -1;
    }
    const timings = this.mode() === 'sync' ? this.editTimings() : (this.data()?.timings ?? []);
    const timing = timings.find((t) => t.mediumUrl === url);
    if (!timing) {
      return -1;
    }
    const now = this.player.progress()?.currentTime ?? 0;
    let active = -1;
    for (let i = 0; i < timing.startTimes.length; i++) {
      const start = timing.startTimes[i];
      if (start != null && start <= now) {
        active = i;
      }
    }
    return active;
  });

  protected startSync(): void {
    const current = this.data()?.timings ?? [];
    this.editTimings.set(current.map((t) => ({ mediumUrl: t.mediumUrl, startTimes: [...t.startTimes] })));
    this.mode.set('sync');
  }

  protected cancel(): void {
    this.mode.set('view');
  }

  /** Stamp the current playback time onto line {@link index} for the playing recording. */
  protected markStart(index: number): void {
    const url = this.syncUrl();
    if (!url) {
      return;
    }
    const now = this.player.progress()?.currentTime ?? 0;
    const lineCount = this.lines().length;
    this.editTimings.update((timings) => {
      const next = timings.map((t) => ({ mediumUrl: t.mediumUrl, startTimes: [...t.startTimes] }));
      let timing = next.find((t) => t.mediumUrl === url);
      if (!timing) {
        timing = { mediumUrl: url, startTimes: [] };
        next.push(timing);
      }
      // Keep the array aligned to the current line count before stamping.
      while (timing.startTimes.length < lineCount) {
        timing.startTimes.push(null);
      }
      timing.startTimes.length = lineCount;
      timing.startTimes[index] = now;
      return next;
    });
  }

  /** The recorded start time of line {@link index} for the playing recording, or `null`. */
  protected startTimeForLine(index: number): number | null {
    const url = this.syncUrl();
    if (!url) {
      return null;
    }
    return this.editTimings().find((t) => t.mediumUrl === url)?.startTimes[index] ?? null;
  }

  protected async save(): Promise<void> {
    if (this.saving()) {
      return;
    }
    const lineCount = this.lines().length;
    // Normalise every recording's array to the current line count, then drop recordings with no timing.
    const timings = this.editTimings()
      .map((t) => ({ mediumUrl: t.mediumUrl, startTimes: Array.from({ length: lineCount }, (_, i) => t.startTimes[i] ?? null) }))
      .filter((t) => t.startTimes.some((s) => s != null));

    this.saving.set(true);
    try {
      const saved = await this.service.saveTimings(this.songId(), timings);
      this.data.update((d) => (d ? { ...d, timings: saved } : d));
      this.mode.set('view');
    } catch {
      // Keep sync mode open on failure.
    } finally {
      this.saving.set(false);
    }
  }

  /** `m:ss`, or blank when unset. */
  protected formatTime(seconds: number | null): string {
    if (seconds == null || !isFinite(seconds)) {
      return '';
    }
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s.toString().padStart(2, '0')}`;
  }
}
