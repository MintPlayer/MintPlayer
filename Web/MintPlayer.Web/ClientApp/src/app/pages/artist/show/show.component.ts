import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ArtistService } from '../../../services/artist/artist.service';
import { Artist } from '../../../interfaces/artist';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit {

  constructor(private artistService: ArtistService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.artistService.getArtist(id, true).subscribe(artist => {
      this.artist = artist;
      if (artist != null) {
        this.titleService.setTitle(artist.name);
      }
    });
  }

  public deleteArtist() {
    this.artistService.deleteArtist(this.artist).subscribe(() => {
      this.router.navigate(["artist"]);
    });
  }

  public artist: Artist = {
    id: 0,
    name: '',
    yearStarted: null,
    yearQuit: null,
    currentMembers: [],
    pastMembers: [],
    media: [],
    songs: [],
    text: ''
  };

  ngOnInit() {
  }

}
