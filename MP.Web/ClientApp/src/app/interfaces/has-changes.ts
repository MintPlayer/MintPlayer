import { IBeforeUnloadEvent } from '../events/my-before-unload.event';

export interface HasChanges {
  hasChanges: boolean;
  beforeUnload: (event: IBeforeUnloadEvent) => void;
}
