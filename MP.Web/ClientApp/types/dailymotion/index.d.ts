declare namespace DM {
  export const enum PlayerState {
    ENDED = 0,
    PLAYING = 1,
    PAUSED = 2
  }

  export interface PlayerOptions {
    video: string;
    width: string;
    height: string;
    params: Partial<PlayerParams>;
    events: Partial<PlayerEvents>;
  }

  export type LoadOptions = Partial<PlayerParams> & {
    video: string;
    playlist?: string;
  }

  export type PlayerParams = {
    'autoplay': boolean;
    'queue-autoplay-next': boolean;
    'queue-enable': boolean;
    'mute': boolean;
    'start': number;
  }

  export interface Player {
    width: number;
    height: number;

    load: (options: LoadOptions) => void;

    currentTime: number;
    duration: number;

    play: () => void;
    pause: () => void;
    seek: (t: number) => void;

    muted: boolean;
    setMuted(mute: boolean);

    volume: number;
    setVolume(volume: number);
    onvolumechange();
  }

  export class PlayerEvents {
    //constructor(data?: Partial<PlayerEvents>) {
    //  Object.assign(this, data);
    //}

    apiready: () => void;
    play: () => void;
    pause: () => void;
    end: () => void;
  }

  export function player(element: HTMLElement, options: Partial<PlayerOptions>): Player;
}
