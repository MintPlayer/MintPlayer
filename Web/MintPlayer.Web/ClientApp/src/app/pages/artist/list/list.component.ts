import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ArtistService } from '../../../services/artist/artist.service';
import { Artist } from '../../../interfaces/artist';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {

  constructor(private artistService: ArtistService, private router: Router, private titleService: Title) {
    this.titleService.setTitle('Artists');
    this.loadArtists();
  }

  ngOnInit() {
  }

  private loadArtists() {
    this.artistService.getArtists(false).subscribe((artists) => {
      this.artists = artists;
    });
  }

  public artists: Artist[] = [];
}
