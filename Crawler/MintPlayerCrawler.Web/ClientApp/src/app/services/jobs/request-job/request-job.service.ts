import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RequestJob } from '../../../entities/jobs/request-job';

@Injectable({
  providedIn: 'root'
})
export class RequestJobService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public createRequestJob(job: RequestJob) {
    return this.httpClient.post<RequestJob>(`${this.baseUrl}/api/RequestJob`, { job }).toPromise();
  }
}
