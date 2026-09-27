import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PlayerService } from '../player/player.service';
import { RoomSyncService } from './room-sync.service';

/**
 * Listen-together room page (spike S9). `/room` creates a room (signed-in hosts only, D31); `/room/:id` joins
 * one — anonymous listeners welcome — and hands playback of the global player to {@link RoomSyncService}.
 * Query params for measurement: `?name=` (guest display name), `?mute=1` (mute so headless autoplay works).
 */
@Component({
  selector: 'app-room-page',
  imports: [FormsModule],
  providers: [RoomSyncService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (!roomId()) {
      <h1>Listen together</h1>
      <p class="text-body-secondary">Open a room, share the link, and everyone hears the same thing at the same time.</p>
      <div class="d-flex gap-2" style="max-width: 32rem">
        <input class="form-control" placeholder="Room name (optional)" [(ngModel)]="newName" />
        <button type="button" class="btn btn-primary text-nowrap" (click)="create()">Open a room</button>
      </div>
      @if (createError()) { <div class="alert alert-warning mt-3">{{ createError() }}</div> }
    } @else {
      @let s = sync.state();
      <div class="d-flex align-items-center gap-2 mb-2">
        <h1 class="h3 mb-0">{{ s?.name ?? 'Room' }}</h1>
        <span class="badge" [class.text-bg-success]="sync.connected()" [class.text-bg-secondary]="!sync.connected()">
          {{ sync.connected() ? 'live' : 'reconnecting…' }}
        </span>
        @if (sync.you()?.isHost) { <span class="badge text-bg-primary">host</span> }
      </div>
      @if (sync.lastError()) { <div class="alert alert-warning py-2">{{ sync.lastError() }}</div> }

      @if (s) {
        <p class="text-body-secondary mb-2">
          Hosted by {{ s.hostName }}{{ s.hostOnline ? '' : ' (offline)' }} · {{ s.listeners.length }} listening
          · you are {{ sync.you()?.displayName }}
        </p>

        <div class="card mb-3">
          <div class="card-body">
            <div class="fw-semibold">{{ sync.currentItem()?.title ?? 'Nothing playing' }}</div>
            <div class="small text-body-secondary font-monospace" data-testid="sync-stats">
              room {{ fmt(sync.expectedPosition()) }}s · {{ s.anchor.playing ? 'playing' : 'paused' }}
              · drift {{ fmt(sync.driftSec()) }}s · clock offset {{ sync.clockOffsetMs().toFixed(0) }} ms
              · rtt {{ sync.rttMs()?.toFixed(0) ?? '–' }} ms
            </div>
            <div class="d-flex flex-wrap gap-2 mt-2">
              @if (sync.you()?.isHost) {
                <button type="button" class="btn btn-sm btn-outline-primary" (click)="sync.send({ type: s.anchor.playing ? 'pause' : 'play' })">
                  {{ s.anchor.playing ? 'Pause' : 'Play' }}
                </button>
                <button type="button" class="btn btn-sm btn-outline-primary" (click)="seekBy(-10)">−10 s</button>
                <button type="button" class="btn btn-sm btn-outline-primary" (click)="seekBy(10)">+10 s</button>
                <button type="button" class="btn btn-sm btn-outline-primary" (click)="sync.send({ type: 'next' })">Next</button>
                <button type="button" class="btn btn-sm btn-outline-secondary" (click)="sync.send({ type: 'lock', locked: !s.locked })">
                  {{ s.locked ? 'Reopen to new listeners' : 'Close to new listeners' }}
                </button>
              }
              @if (sync.you()?.signedIn) {
                <button type="button" class="btn btn-sm btn-outline-secondary" (click)="sync.send({ type: 'voteSkip' })">
                  Vote skip ({{ s.skipVotes }}/{{ s.votesNeeded }})
                </button>
              }
              <button type="button" class="btn btn-sm btn-outline-secondary" (click)="player.setMuted(!player.muted())">
                {{ player.muted() ? 'Unmute' : 'Mute' }}
              </button>
            </div>
          </div>
        </div>

        @if (sync.you()?.signedIn) {
          <div class="d-flex gap-2 mb-3" style="max-width: 40rem">
            <input class="form-control" placeholder="YouTube / SoundCloud / … url" [(ngModel)]="addUrl" />
            <button type="button" class="btn btn-outline-primary text-nowrap" (click)="add()">Add to queue</button>
          </div>
        } @else {
          <p class="small text-body-secondary">Sign in to add songs or vote to skip.</p>
        }

        <ol class="list-group list-group-numbered">
          @for (item of s.queue; track item.id) {
            <li class="list-group-item" [class.active]="item.id === s.currentItemId">
              {{ item.title }} <span class="small opacity-75">— {{ item.addedBy }}</span>
            </li>
          }
        </ol>
      }
    }
  `,
})
export class RoomPage implements OnInit, OnDestroy {
  protected readonly sync = inject(RoomSyncService);
  protected readonly player = inject(PlayerService);
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  protected readonly roomId = signal<string | null>(null);
  protected readonly createError = signal<string | null>(null);
  protected newName = '';
  protected addUrl = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.roomId.set(id);
    if (!id || !this.isBrowser) return;

    const q = this.route.snapshot.queryParamMap;
    if (q.get('mute') === '1') this.player.setMuted(true);
    this.sync.join(id, q.get('name') ?? 'Guest');

    // Spike instrumentation (primitives only — read through JSON.parse in the test harness).
    (window as unknown as Record<string, unknown>)['__room'] = {
      stats: () => JSON.stringify(this.stats()),
      samples: () => JSON.stringify(this.sync.samples),
      seeks: () => JSON.stringify(this.sync.seeks),
      events: () => JSON.stringify(this.sync.events),
      stall: (sec: number) => this.sync.simulateStall(sec),
      send: (json: string) => this.sync.send(JSON.parse(json)),
      reset: () => { this.sync.samples.length = 0; this.sync.seeks.length = 0; this.sync.events.length = 0; },
    };
  }

  ngOnDestroy(): void {
    if (this.isBrowser) delete (window as unknown as Record<string, unknown>)['__room'];
  }

  private stats() {
    const d = this.sync.samples.map((s) => Math.abs(s.d));
    return {
      connected: this.sync.connected(),
      isHost: this.sync.you()?.isHost ?? false,
      expected: this.sync.expectedPosition(),
      local: this.player.livePosition(),
      drift: this.sync.driftSec(),
      playerState: this.player.playerState(),
      offsetMs: this.sync.clockOffsetMs(),
      rttMs: this.sync.rttMs(),
      samples: d.length,
      maxAbsDrift: d.length ? Math.max(...d) : null,
      avgAbsDrift: d.length ? d.reduce((a, b) => a + b, 0) / d.length : null,
      seeks: this.sync.seeks.length,
      replays: this.sync.replays,
      reconnects: this.sync.reconnects,
      item: this.sync.currentItem()?.id ?? null,
    };
  }

  protected fmt(n: number | null): string {
    return n === null ? '–' : n.toFixed(2);
  }

  protected seekBy(delta: number): void {
    const pos = this.sync.expectedPosition();
    if (pos !== null) this.sync.send({ type: 'seek', positionSec: Math.max(0, pos + delta) });
  }

  protected add(): void {
    const url = this.addUrl.trim();
    if (!url) return;
    this.sync.send({ type: 'add', url });
    this.addUrl = '';
  }

  protected async create(): Promise<void> {
    this.createError.set(null);
    try {
      const res = await firstValueFrom(this.http.post<{ id: string }>('/api/rooms', { name: this.newName || null }));
      await this.router.navigate(['/room', res.id]); // distinct route → fresh component instance
    } catch (e) {
      const status = (e as HttpErrorResponse).status;
      this.createError.set(status === 401 ? 'Sign in to open a room.' : `Could not open a room (${status}).`);
    }
  }
}
