import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { SongService } from '../../../services/song/song.service';
import { Song } from '../../../interfaces/song';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit {
  constructor(private songService: SongService, @Inject('SONG') private songInj: Song, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    if (songInj === null) {
      this.loadSong(id);
    } else {
      this.setSong(songInj);
    }
  }

  private loadSong(id: number) {
    this.songService.getSong(id, true).subscribe(song => {
      this.setSong(song);
    });
  }

  @Output() addToPlaylist: EventEmitter<Song> = new EventEmitter();

  private setSong(song: Song) {
    this.song = song;
    if (song != null) {
      this.titleService.setTitle(`${song.title}: Video and lyrics`);
    }
  }

  public doAddToPlaylist() {
    this.addToPlaylist.emit(this.song);
  }

  ngOnInit() {
    this.route.params.subscribe(routeParams => {
    	this.loadSong(routeParams.id);
    });
  }

  public deleteSong() {
    this.songService.deleteSong(this.song).subscribe(() => {
      this.router.navigate(["song"]);
    });
  }

  public song: Song = {
    id: 0,
    title: "",
    released: null,
    artists: [],
    media: [],
    lyrics: "",
    text: "",
    description: "",
    youtubeId: ""
  };
}
