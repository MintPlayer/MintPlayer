import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { Artist } from '../../../interfaces/artist';
import { Person } from '../../../interfaces/person';
import { ArtistService } from '../../../services/artist/artist.service';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {

  constructor(private artistService: ArtistService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.artistService.getArtist(id, true).subscribe((artist) => {
      this.artist = artist;
      this.titleService.setTitle(`Edit artist: ${artist.name}`);
      this.oldName = artist.name;
    });
  }

  oldName: string = '';
  artist: Artist = {
    id: 0,
    name: '',
    yearStarted: null,
    yearQuit: null,
    currentMembers: [],
    pastMembers: [],
    songs: [],
    media: [],
    text: ''
  }

  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  public CurrentMemberChanged(person: [Person, string]) {
    var action = person[1]; // add, remove
    switch (action) {
      case 'add':
        this.artist.currentMembers.push(person[0]);
        break;
      case 'remove':
        this.artist.currentMembers.splice(this.artist.currentMembers.indexOf(person[0]), 1);
        break;
    }
  }

  public PastMemberChanged(person: [Person, string]) {
    var action = person[1]; // add, remove
    switch (action) {
      case 'add':
        this.artist.pastMembers.push(person[0]);
        break;
      case 'remove':
        this.artist.pastMembers.splice(this.artist.pastMembers.indexOf(person[0]), 1);
        break;
    }
  }

  public updateArtist() {
    this.artistService.updateArtist(this.artist).subscribe(() => {
      this.router.navigate(['artist', this.artist.id]);
    });
  }

  ngOnInit() {
  }

}
