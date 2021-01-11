import { eJobStatus } from '../../enums/eJobStatus';

export abstract class Job {
  id: number;
  status: eJobStatus;
}
