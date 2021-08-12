import { Component, OnInit, Input } from '@angular/core';
import { Medium } from '@mintplayer/ng-client';
import { PlayerTypeFinderService } from '@mintplayer/ng-video-player';

@Component({
  selector: 'media-list',
  templateUrl: './media-list.component.html',
  styleUrls: ['./media-list.component.scss']
})
export class MediaListComponent implements OnInit {

  constructor(private playerTypeFinder: PlayerTypeFinderService) {
  }

  ngOnInit() {
  }

  isPlayable(url: string) {
    return (this.playerTypeFinder.getPlatformWithId(url) !== null);
  }

  @Input() media: Medium[];
}
