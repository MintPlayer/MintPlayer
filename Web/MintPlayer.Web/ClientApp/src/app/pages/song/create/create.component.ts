import { Component, OnInit } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Song } from '../../../interfaces/song';
import { Artist } from '../../../interfaces/artist';
import { SongService } from '../../../services/song/song.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../interfaces/medium-type';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  constructor(private songService: SongService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    this.titleService.setTitle('Add new song');
    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
  }

  ngOnInit() {
  }

  mediumTypes: MediumType[] = [];
  oldSongTitle: string = "";
  song: Song = {
    id: 0,
    title: "",
    released: null,
    artists: [],
    media: [],
    lyrics: "",
    text: ""
  };

  public artistChanged(artist: [Artist, string]) {
    var action = artist[1]; // add, remove
    switch (action) {
      case 'add':
        this.song.artists.push(artist[0]);
        break;
      case 'remove':
        this.song.artists.splice(this.song.artists.indexOf(artist[0]), 1);
        break;
    }
  }

  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  saveSong() {
    this.songService.createSong(this.song).subscribe((song) => {
      this.router.navigate(["song", song.id]);
    });
  }

}
