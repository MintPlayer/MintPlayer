import { Component, OnInit } from '@angular/core';
import { ArtistService } from '../../../services/artist/artist.service';
import { Router } from '@angular/router';
import { Artist } from '../../../interfaces/artist';
import { Person } from '../../../interfaces/person';
import { HttpHeaders } from '@angular/common/http';
import { MediumType } from '../../../interfaces/medium-type';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {
  constructor(private artistService: ArtistService, private mediumTypeService: MediumTypeService, private router: Router) {
    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
  }

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
  };

  currentMemberChanged(person: [Person, string]) {
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

  pastMemberChanged(person: [Person, string]) {
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

  saveArtist() {
    this.artistService.createArtist(this.artist).subscribe((artist) => {
      this.router.navigate(['/artist', artist.id]);
    })
  }

  mediumTypes: MediumType[] = [];
  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true),
    'Content-Type': 'application/json'
  });

  ngOnInit() {
  }
}
