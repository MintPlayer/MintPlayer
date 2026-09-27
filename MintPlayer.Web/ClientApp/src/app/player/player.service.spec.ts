import { TestBed } from '@angular/core/testing';
import { EPlayerState } from '@mintplayer/player-provider';
import { ERepeatMode } from '@mintplayer/playlist-controller';
import { PlayerService } from './player.service';
import { PlaylistEntry } from './playlist-entry';

const entry = (k: string): PlaylistEntry => ({ key: k, url: `https://youtu.be/${k}`, title: k.toUpperCase() });

describe('PlayerService', () => {
  let player: PlayerService;

  beforeEach(() => {
    // providedIn:'root' + a fresh TestBed injector per test → no state leaks between tests.
    player = TestBed.inject(PlayerService);
  });

  it('playNow loads the first entry and fills the queue', () => {
    player.playNow([entry('a'), entry('b')]);
    expect(player.currentEntry()?.key).toBe('a');
    expect(player.queue().length).toBe(2);
    expect(player.hasCurrent()).toBe(true);
  });

  it('addToQueue appends without changing the current track', () => {
    player.playNow([entry('a')]);
    player.addToQueue([entry('b'), entry('c')]);
    expect(player.currentEntry()?.key).toBe('a');
    expect(player.queue().map((e) => e.key)).toEqual(['a', 'b', 'c']);
  });

  it('auto-advances on EPlayerState.ended and exhausts to null (noRepeat)', () => {
    player.playNow([entry('a'), entry('b')]);
    player.onPlayerState(EPlayerState.ended);
    expect(player.currentEntry()?.key).toBe('b');
    player.onPlayerState(EPlayerState.ended);
    expect(player.currentEntry()).toBe(null);
    expect(player.hasCurrent()).toBe(false);
  });

  it('removes via a queue() instance and advances when it was current', () => {
    player.playNow([entry('a'), entry('b'), entry('c')]);
    player.remove(player.queue()[0]); // the current 'a'
    expect(player.currentEntry()?.key).toBe('b');
    expect(player.queue().map((e) => e.key)).toEqual(['b', 'c']);
  });

  it('cycleRepeat walks noRepeat → repeatOne → repeatAll → noRepeat', () => {
    expect(player.repeat()).toBe(ERepeatMode.noRepeat);
    player.cycleRepeat();
    expect(player.repeat()).toBe(ERepeatMode.repeatOne);
    player.cycleRepeat();
    expect(player.repeat()).toBe(ERepeatMode.repeatAll);
    player.cycleRepeat();
    expect(player.repeat()).toBe(ERepeatMode.noRepeat);
  });

  it('setShuffle and togglePlayPause update state', () => {
    player.setShuffle(true);
    expect(player.shuffle()).toBe(true);

    player.playNow([entry('a')]);
    player.onPlayerState(EPlayerState.playing);
    expect(player.isPlaying()).toBe(true);
    player.togglePlayPause();
    expect(player.playerState()).toBe(EPlayerState.paused);
  });

  it('onProgress feeds the progress signal', () => {
    player.onProgress({ currentTime: 12, duration: 200 });
    expect(player.progress()?.currentTime).toBe(12);
  });

  it('toggleSidebar flips isOpen', () => {
    expect(player.isOpen()).toBe(false);
    player.toggleSidebar();
    expect(player.isOpen()).toBe(true);
    player.toggleSidebar();
    expect(player.isOpen()).toBe(false);
  });

  it('playNow optimistically marks playback as playing (so [playerState] agrees with autoplay)', () => {
    player.playNow([entry('a')]);
    expect(player.isPlaying()).toBe(true);
  });

  it('addToQueue starts playing only when the queue was empty', () => {
    player.addToQueue([entry('a')]); // empty → starts
    expect(player.isPlaying()).toBe(true);
    player.onPlayerState(EPlayerState.paused);
    player.addToQueue([entry('b')]); // not empty → leaves state alone
    expect(player.playerState()).toBe(EPlayerState.paused);
  });

  it('displayTitle overlays a resolved title and falls back to the entry title', () => {
    player.playNow([entry('a')]);
    const current = player.currentEntry()!;
    expect(player.displayTitle(current)).toBe('A'); // placeholder from the entry
    player.setResolvedTitle('a', '  Bohemian Rhapsody  ');
    expect(player.displayTitle(current)).toBe('Bohemian Rhapsody'); // resolved + trimmed
    player.setResolvedTitle('a', '   '); // blank is ignored
    expect(player.displayTitle(current)).toBe('Bohemian Rhapsody');
    expect(player.displayTitle(null)).toBe('');
  });
});
