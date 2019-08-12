import { Component, OnInit, Input } from '@angular/core';
import { SubjectService } from '../../../services/subject/subject.service';
import { Subject } from '../../../interfaces/subject';
import { SubjectLikeResponse } from '../../../interfaces/subjectLikeResponse';

@Component({
  selector: 'subject-like',
  templateUrl: './subject-like.component.html',
  styleUrls: ['./subject-like.component.scss']
})
export class SubjectLikeComponent implements OnInit {
  constructor(private subjectService: SubjectService) {
  }

  private _subject: Subject;
  get subject(): Subject {
    return this._subject;
  }
  @Input() set subject(subject: Subject) {
    this._subject = subject;
    this.loading = true;
    this.subjectService.getLikes(subject).subscribe((data) => {
      this.data = data;
      this.loading = false;
    });
  }

  public data: SubjectLikeResponse = {
    likes: 0,
    dislikes: 0,
    like: null,
    authenticated: false
  };

  public like(like: boolean) {
    this.subjectService.like(this.subject, like).subscribe((data) => {
      this.data = data;
    });
  }

  ngOnInit() {
  }

  public loading: boolean = false;
}
