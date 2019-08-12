import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { Song } from '../../../interfaces/song';
import { Artist } from '../../../interfaces/artist';
import { SongService } from '../../../services/song/song.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../interfaces/medium-type';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {
  constructor(private songService: SongService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.songService.getSong(id, true).subscribe(song => {
      this.song = song;
      this.titleService.setTitle(`Edit song: ${song.title}`);
      this.oldSongTitle = song.title;
    });
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

  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  artistChanged(artist: [Artist, string]) {
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

  updateSong() {
    this.songService.updateSong(this.song).subscribe(() => {
      this.router.navigate(["song", this.song.id]);
    });
  }
}
