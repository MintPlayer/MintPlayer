import { Injectable, OnDestroy, computed, effect, inject, signal, untracked } from '@angular/core';
import { EPlayerState } from '@mintplayer/player-provider';
import { PlayerService } from '../player/player.service';
import { playlistEntryFromUrl } from '../player/media-resolver';
import { ClientMessage, RoomQueueItem, RoomState, RoomYou, ServerMessage } from './room-protocol';

/** Seek when the local player is further than this from the room position. */
export const DRIFT_THRESHOLD_SEC = 0.75;
/** After a seek / (re)load, give the player this long to settle before judging drift again. */
const SETTLE_MS = 2500;
/** Minimum spacing between re-issued play/pause commands. */
const COMMAND_COOLDOWN_MS = 1500;
const TICK_MS = 250;
/** A drift above this that persists for SOFT_DRIFT_MS is also corrected. */
const SOFT_DRIFT_THRESHOLD_SEC = 0.3;
const SOFT_DRIFT_MS = 8000;

export interface DriftSample {
  /** Estimated server time (epoch ms) of the sample. */
  t: number;
  /** local − expected, seconds. */
  d: number;
}

/**
 * Listen-together client (spike S9), provided per room page. Owns the room socket and drives the global
 * {@link PlayerService} from the room's playback anchor:
 *
 * - **Clock sync**: `ping{t0}` → `pong{t0,t1}`; offset = t1 − (t0+t2)/2, keeping the lowest-RTT sample of the
 *   last 8 (NTP-style). 5 quick pings on (re)connect, then one every 10 s.
 * - **Drift correction** every 250 ms: expected = anchor.positionSec + (serverNow − anchor.serverTimeMs)/1000
 *   when playing; local = the player's last `currentTime` extrapolated. |local − expected| > 0.75 s → seek to
 *   expected (+ an adaptive seek lead that learns how late a seek lands), then settle 2.5 s.
 * - **State following**: if the room plays but the player is paused/unstarted (user pause, ad, stall), re-command
 *   play; on the player's `playing` edge (end of an ad / buffering) re-check drift immediately.
 * - **Reconnect** with exponential backoff (1 s → 10 s); the server replays full state in `welcome`.
 */
@Injectable()
export class RoomSyncService implements OnDestroy {
  private readonly player = inject(PlayerService);

  readonly state = signal<RoomState | null>(null);
  readonly you = signal<RoomYou | null>(null);
  readonly connected = signal(false);
  readonly lastError = signal<string | null>(null);
  readonly fatal = signal(false);

  /** Estimated serverClock − localClock (ms) and the RTT of the sample it came from. */
  readonly clockOffsetMs = signal(0);
  readonly rttMs = signal<number | null>(null);
  readonly driftSec = signal<number | null>(null);

  readonly currentItem = computed<RoomQueueItem | null>(() => {
    const s = this.state();
    return s?.queue.find((i) => i.id === s.currentItemId) ?? null;
  });

  // ----- measurement (exposed on window.__room for the spike) -----
  readonly samples: DriftSample[] = [];
  readonly seeks: { t: number; drift: number; target: number }[] = [];
  readonly events: { t: number; e: string }[] = [];
  reconnects = 0;
  replays = 0;

  private roomId = '';
  private name = '';
  private socket: WebSocket | null = null;
  private backoffMs = 1000;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private pingTimer: ReturnType<typeof setInterval> | null = null;
  private tickTimer: ReturnType<typeof setInterval> | null = null;
  private destroyed = false;
  private clockSamples: { offset: number; rtt: number }[] = [];

  private loadedItemId: string | null = null;
  private settleUntil = 0;
  private lastCommandAt = 0;
  private lastSeek: { at: number; target: number } | null = null;
  /** Learned lateness of a seek (s): a YouTube seek re-buffers, so it lands behind the target. */
  private seekLead = 0.2;
  private reportedEndedFor: string | null = null;
  /** The item the player last reached `playing` for (guards against a stale `ended`). */
  private playedItemId: string | null = null;
  /** performance.now() since which |drift| has continuously exceeded the soft threshold (0 = not). */
  private softDriftSince = 0;
  /** When > now, drift correction is suspended (stall simulation). */
  private suspendedUntil = 0;

  constructor() {
    // Re-sync the moment the player (re)starts playing — end of an ad, buffering, or a local pause.
    let previous: EPlayerState | null = null;
    effect(() => {
      const ps = this.player.playerState();
      untracked(() => {
        if (ps === EPlayerState.playing && previous !== EPlayerState.playing) {
          this.log(`player playing`);
          this.playedItemId = this.loadedItemId;
          this.settleUntil = Math.min(this.settleUntil, performance.now() + 500);
        } else if (ps === EPlayerState.ended) {
          const item = this.currentItem();
          // Only an end the player reached after playing THIS item counts: when two consecutive items share a
          // url the player does not reload and can re-report the previous item's `ended` right after the switch.
          // Also require the position to actually be at the end: YouTube was observed to emit a spurious
          // ENDED mid-video (around a blocked ad break), which would otherwise skip the track for the room.
          const progress = this.player.progress();
          const expected = this.expectedPosition() ?? 0;
          const duration = progress?.duration ?? 0;
          const atEnd = duration > 0 && Math.max(progress?.currentTime ?? 0, expected) >= duration - 3;
          this.log(`player ended (pos ${progress?.currentTime?.toFixed(1)} / ${duration.toFixed(1)}, room ${expected.toFixed(1)})`);
          if (item && this.playedItemId === item.id && this.reportedEndedFor !== item.id && atEnd) {
            this.reportedEndedFor = item.id;
            this.send({ type: 'ended', itemId: item.id, durationSec: duration });
          }
        } else if (ps !== previous) {
          this.log(`player ${EPlayerState[ps]}`);
        }
        previous = ps;
      });
    });
  }

  // ----- lifecycle -----

  join(roomId: string, displayName: string): void {
    this.roomId = roomId;
    this.name = displayName;
    this.connect();
    this.tickTimer = setInterval(() => this.tick(), TICK_MS);
    this.pingTimer = setInterval(() => this.ping(), 10_000);
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    if (this.reconnectTimer) clearTimeout(this.reconnectTimer);
    if (this.pingTimer) clearInterval(this.pingTimer);
    if (this.tickTimer) clearInterval(this.tickTimer);
    this.socket?.close();
    this.player.clear();
  }

  private connect(): void {
    const scheme = location.protocol === 'https:' ? 'wss' : 'ws';
    const url = `${scheme}://${location.host}/ws/rooms/${encodeURIComponent(this.roomId)}?name=${encodeURIComponent(this.name)}`;
    const ws = new WebSocket(url);
    this.socket = ws;

    ws.onopen = () => {
      this.connected.set(true);
      this.backoffMs = 1000;
      this.log('socket open');
      for (let i = 0; i < 5; i++) setTimeout(() => this.ping(), i * 200);
    };
    ws.onmessage = (ev) => this.onMessage(JSON.parse(ev.data) as ServerMessage);
    ws.onclose = () => {
      this.connected.set(false);
      this.log('socket closed');
      if (this.destroyed || this.fatal()) return;
      this.reconnects++;
      this.reconnectTimer = setTimeout(() => this.connect(), this.backoffMs);
      this.backoffMs = Math.min(this.backoffMs * 2, 10_000);
    };
  }

  private onMessage(msg: ServerMessage): void {
    switch (msg.type) {
      case 'welcome':
        this.you.set(msg.you);
        this.lastError.set(null);
        this.log('welcome');
        this.applyState(msg.state);
        break;
      case 'state':
        this.applyState(msg.state);
        break;
      case 'pong': {
        const t2 = this.localNow();
        const rtt = t2 - msg.t0;
        const offset = msg.t1 - (msg.t0 + t2) / 2;
        this.clockSamples.push({ offset, rtt });
        if (this.clockSamples.length > 8) this.clockSamples.shift();
        const best = this.clockSamples.reduce((a, b) => (b.rtt < a.rtt ? b : a));
        this.clockOffsetMs.set(best.offset);
        this.rttMs.set(best.rtt);
        break;
      }
      case 'error':
        this.lastError.set(msg.message);
        if (msg.fatal) this.fatal.set(true);
        break;
    }
  }

  private applyState(state: RoomState): void {
    const prev = this.state();
    this.state.set(state);
    // Any anchor change (play/pause/seek by the host) must take effect now, not after a settle window.
    if (!prev || prev.anchor.serverTimeMs !== state.anchor.serverTimeMs || prev.currentItemId !== state.currentItemId) {
      this.settleUntil = 0;
      this.lastCommandAt = 0;
    }
    this.tick();
  }

  // ----- commands -----

  send(msg: ClientMessage): void {
    if (this.socket?.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(msg));
    }
  }

  private ping(): void {
    this.send({ type: 'ping', t0: this.localNow() });
  }

  /** Room position (s) right now, from the anchor and the estimated server clock. */
  expectedPosition(): number | null {
    const s = this.state();
    if (!s?.currentItemId) return null;
    const a = s.anchor;
    return a.playing ? a.positionSec + (this.serverNow() - a.serverTimeMs) / 1000 : a.positionSec;
  }

  serverNow(): number {
    return this.localNow() + this.clockOffsetMs();
  }

  private localNow(): number {
    return performance.timeOrigin + performance.now();
  }

  // ----- the sync loop -----

  private tick(): void {
    const s = this.state();
    if (!s) return;
    const now = performance.now();
    const item = this.currentItem();

    if (!item) {
      if (this.loadedItemId !== null) {
        this.loadedItemId = null;
        this.player.clear();
      }
      this.driftSec.set(null);
      return;
    }

    if (this.loadedItemId !== item.id || !this.player.hasCurrent()) {
      this.loadedItemId = item.id;
      this.reportedEndedFor = null;
      this.playedItemId = null;
      this.player.playNow([playlistEntryFromUrl(item.url, { key: `room:${item.id}`, title: item.title })]);
      if (!s.anchor.playing) this.player.setPlaying(false);
      this.settleUntil = now + SETTLE_MS;
      this.lastSeek = null;
      this.log(`load ${item.id}`);
      return;
    }

    if (now < this.suspendedUntil) return;

    const localState = this.player.playerState();
    const playing = localState === EPlayerState.playing;

    // Follow the room's play/pause.
    if (now - this.lastCommandAt > COMMAND_COOLDOWN_MS) {
      const duration = this.player.progress()?.duration ?? 0;
      const genuinelyEnded = localState === EPlayerState.ended && (duration <= 0 || (this.expectedPosition() ?? 0) >= duration - 3);
      if (s.anchor.playing && !playing && !genuinelyEnded) {
        this.player.setPlaying(true);
        this.lastCommandAt = now;
        this.replays++;
        this.log(`re-play (was ${EPlayerState[localState]})`);
      } else if (!s.anchor.playing && playing) {
        this.player.setPlaying(false);
        this.lastCommandAt = now;
      }
    }

    const expected = this.expectedPosition();
    const local = this.player.livePosition();
    if (expected === null || local === null) return;
    const drift = local - expected;
    this.driftSec.set(drift);

    if (now < this.settleUntil) return;

    // Learn how late the previous seek landed (drift right after settling): EWMA, clamped to [0, 1.5] s.
    if (this.lastSeek && playing) {
      this.seekLead = Math.min(1.5, Math.max(0, this.seekLead - 0.5 * drift));
      this.lastSeek = null;
    }

    const shouldBePlaying = s.anchor.playing;
    if (shouldBePlaying && !playing) return; // re-play pending; judge drift once it plays

    // Hard threshold: seek at once. Soft threshold: a steady offset (typically start-up latency that landed
    // just under the hard threshold) is corrected once it has persisted for SOFT_DRIFT_MS.
    const soft = Math.abs(drift) > SOFT_DRIFT_THRESHOLD_SEC;
    this.softDriftSince = soft ? this.softDriftSince || now : 0;
    const persistent = soft && now - this.softDriftSince > SOFT_DRIFT_MS;

    if (Math.abs(drift) > DRIFT_THRESHOLD_SEC || persistent) {
      this.softDriftSince = 0;
      const target = Math.max(0, expected + (shouldBePlaying ? this.seekLead : 0));
      if (this.player.seek(target)) {
        this.seeks.push({ t: Math.round(this.serverNow()), drift: round3(drift), target: round3(target) });
        this.log(`seek drift=${drift.toFixed(2)} -> ${target.toFixed(2)} (lead ${this.seekLead.toFixed(2)})`);
        this.lastSeek = { at: now, target };
        this.settleUntil = now + SETTLE_MS;
      }
      return;
    }

    if (shouldBePlaying && playing) {
      this.samples.push({ t: Math.round(this.serverNow()), d: round3(drift) });
    }
  }

  // ----- spike instrumentation -----

  /** Freeze drift correction for `seconds` and pause the player — a stand-in for a long ad / buffering stall. */
  simulateStall(seconds: number): void {
    this.log(`simulate stall ${seconds}s`);
    this.suspendedUntil = performance.now() + seconds * 1000;
    this.player.setPlaying(false);
    setTimeout(() => {
      this.log('stall over');
      this.settleUntil = 0;
      this.lastCommandAt = 0;
      this.tick();
    }, seconds * 1000);
  }

  private log(e: string): void {
    this.events.push({ t: Math.round(this.serverNow()), e });
    if (this.events.length > 500) this.events.shift();
  }
}

function round3(n: number): number {
  return Math.round(n * 1000) / 1000;
}
