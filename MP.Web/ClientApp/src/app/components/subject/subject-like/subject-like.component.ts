import { Component, OnInit, Input, Inject } from '@angular/core';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { SubjectLikeResponse } from '../../../entities/subject-like-response';
import { Subject } from '../../../entities/subject';
import { SubjectService } from '../../../services/subject/subject.service';

@Component({
  selector: 'subject-like',
  templateUrl: './subject-like.component.html',
  styleUrls: ['./subject-like.component.scss']
})
export class SubjectLikeComponent implements OnInit {
  constructor(
    private subjectService: SubjectService,
    @Inject(SERVER_SIDE) private serverSide: boolean
  ) {
  }

  private _subject: Subject;
  get subject(): Subject {
    return this._subject;
  }
  @Input() set subject(subject: Subject) {
    this._subject = subject;
    if (this.serverSide) {

    } else {
      this.loading = true;
      this.subjectService.getLikes(subject).then((data) => {
        this.data = data;
        this.loading = false;
      }).catch((error) => {
        console.error('Could not get like count', error);
      });
    }
  }

  public loading: boolean = false;
  public data: SubjectLikeResponse = {
    likes: 0,
    dislikes: 0,
    like: null,
    authenticated: false
  };

  public like(like: boolean) {
    this.subjectService.like(this.subject, like).then((data) => {
      this.data = data;
    }).catch((error) => {
      console.error('Could not like the subject', error);
    });
  }

  ngOnInit() {
  }
}
