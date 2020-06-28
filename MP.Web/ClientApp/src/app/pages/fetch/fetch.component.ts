import { Component, OnInit, OnDestroy } from '@angular/core';
import { SubjectService } from '../../services/subject/subject.service';

@Component({
  selector: 'app-fetch',
  templateUrl: './fetch.component.html',
  styleUrls: ['./fetch.component.scss']
})
export class FetchComponent implements OnInit, OnDestroy {

  constructor(private subjectService: SubjectService) {
  }

  url: string = '';
  subject: any;
  fetchUrl() {
    this.subjectService.fetch(this.url).then((subject) => {
      this.subject = subject;
    }).catch((error) => {
      console.log(error);
    });
  }

  ngOnInit() {
  }

  ngOnDestroy() {
  }

}
