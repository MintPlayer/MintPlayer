import { Component, OnInit } from '@angular/core';
import { RequestJobService } from '../../services/jobs/request-job/request-job.service';
import { eJobStatus } from '../../enums/eJobStatus';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  constructor(private requestJobService: RequestJobService) {
  }

  urlToAdd: string = '';
  createCrawlJob() {
    this.requestJobService.createRequestJob({
      id: 0,
      url: this.urlToAdd,
      status: eJobStatus.pending
    }).then((job) => {
      debugger;
    }).catch((error) => {
      debugger;
    });
  }

  ngOnInit() {
  }

}
