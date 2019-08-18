import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { SongService } from '../../../services/song/song.service';
import { Song } from '../../../interfaces/song';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  constructor(private songService: SongService, @Inject('SONGS') private songsInj: Song[], private router: Router, private route: ActivatedRoute, private titleService: Title) {
    this.titleService.setTitle('All songs');
    if (songsInj === null) {
      this.loadSongs();
    } else {
      this.songs = songsInj;
    }
  }

  private loadSongs() {
    this.songService.getSongs(false).subscribe(songs => {
      this.songs = songs;
    });
  }

  ngOnInit() {
    //this.route.params.subscribe((routeParams) => {
    //	this.loadSongs();
    //});
  }

  public songs: Song[] = [];
}
