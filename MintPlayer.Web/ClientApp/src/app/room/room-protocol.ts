/** Wire types of the listen-together room socket (`/ws/rooms/{id}`) — mirror of `MintPlayer.Web.Rooms.RoomService`. */

export interface RoomQueueItem {
  id: string;
  url: string;
  title: string;
  addedBy: string;
}

/** "At server time `serverTimeMs` the item `mediaUrl` was at `positionSec`, (not) playing." */
export interface RoomAnchor {
  mediaUrl: string | null;
  positionSec: number;
  serverTimeMs: number;
  playing: boolean;
}

export interface RoomListener {
  displayName: string;
  signedIn: boolean;
  isHost: boolean;
}

export interface RoomState {
  roomId: string;
  name: string;
  hostName: string;
  hostOnline: boolean;
  locked: boolean;
  closed: boolean;
  queue: RoomQueueItem[];
  currentItemId: string | null;
  anchor: RoomAnchor;
  skipVotes: number;
  votesNeeded: number;
  listeners: RoomListener[];
}

export interface RoomYou {
  connectionId: string;
  isHost: boolean;
  signedIn: boolean;
  displayName: string;
}

export type ServerMessage =
  | { type: 'welcome'; you: RoomYou; state: RoomState }
  | { type: 'state'; state: RoomState }
  | { type: 'pong'; t0: number; t1: number }
  | { type: 'error'; code: string; message: string; fatal?: boolean };

export type ClientMessage =
  | { type: 'ping'; t0: number }
  | { type: 'play' }
  | { type: 'pause' }
  | { type: 'seek'; positionSec: number }
  | { type: 'next' }
  | { type: 'ended'; itemId: string; durationSec: number }
  | { type: 'add'; url: string; title?: string }
  | { type: 'voteSkip' }
  | { type: 'lock'; locked: boolean }
  | { type: 'close' };
