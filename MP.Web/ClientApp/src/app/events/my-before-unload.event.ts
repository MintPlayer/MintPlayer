export interface IBeforeUnloadEvent {
  returnValue: string;
  defaultPrevented: boolean;
  preventDefault: () => void;
}

export class MyBeforeUnloadEvent implements IBeforeUnloadEvent {
  constructor() {
  }

  public returnValue: string = null;
  public defaultPrevented: boolean = false;
  public preventDefault() {
    this.defaultPrevented = true;
  }
}
