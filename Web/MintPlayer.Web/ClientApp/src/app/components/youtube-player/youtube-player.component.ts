/// <reference path="../../../../node_modules/@types/youtube/index.d.ts" />

import { Component, OnInit, AfterViewInit, Input } from '@angular/core';
import { YoutubeHelper } from '../../helpers/youtubeHelper';

@Component({
  selector: 'youtube-player',
  templateUrl: './youtube-player.component.html',
  styleUrls: ['./youtube-player.component.scss']
})
export class YoutubePlayerComponent implements OnInit, AfterViewInit {

  constructor(private youtubeHelper: YoutubeHelper) {
  }

  @Input() domId: string;
  @Input() width: number;
  @Input() height: number;

  private player: YT.Player;
  private isApiReady: boolean = false;

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.youtubeHelper.apiReady.subscribe((ready) => {
      this.isApiReady = ready;
      if (ready && !this.player) {
        this.player = new YT.Player(this.domId, {
          videoId: 'EHfx9LXzxpw',
          width: this.width,
          height: this.height,
          playerVars: {
          }
        });
      }
    });
  }

  public playSong(youtubeId: string) {
    if (this.isApiReady) {
      this.player.loadVideoById(youtubeId);
    }
  }
}
