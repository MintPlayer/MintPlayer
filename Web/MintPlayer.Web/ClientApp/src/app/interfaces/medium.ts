import { MediumType } from './medium-type';

export interface Medium {
  id: number;
  value: string;
  type: MediumType;
}
