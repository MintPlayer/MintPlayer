import { Component, OnInit, Input } from '@angular/core';
import { Medium } from '@mintplayer/ng-client';

@Component({
  selector: 'media-list',
  templateUrl: './media-list.component.html',
  styleUrls: ['./media-list.component.scss']
})
export class MediaListComponent implements OnInit {

  constructor() {
  }

  ngOnInit() {
  }

  @Input() media: Medium[];
}
