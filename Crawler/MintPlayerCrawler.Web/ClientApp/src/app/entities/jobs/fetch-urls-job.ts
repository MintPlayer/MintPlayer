import { Job } from './job';

export class FetchUrlsJob extends Job {
  url: string;
  html: string;
}
