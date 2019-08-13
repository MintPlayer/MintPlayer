import { Component, OnInit, Input } from '@angular/core';
import { Medium } from '../../../interfaces/medium';

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
