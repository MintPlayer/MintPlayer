import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { Artist } from '../../../interfaces/artist';
import { Person } from '../../../interfaces/person';
import { ArtistService } from '../../../services/artist/artist.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../interfaces/medium-type';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {

  constructor(private artistService: ArtistService, @Inject('ARTIST') private artistInj: Artist, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    if (artistInj === null) {
      var id = parseInt(this.route.snapshot.paramMap.get("id"));
      this.artistService.getArtist(id, true).subscribe((artist) => {
        this.setArtist(artist);
      });
    } else {
      this.setArtist(artistInj);
    }

    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
  }

  mediumTypes: MediumType[] = [];
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

  private setArtist(artist: Artist) {
    this.artist = artist;
    if (artist !== null) {
      this.titleService.setTitle(`Edit artist: ${artist.name}`);
      this.oldName = artist.name;
    }
  }

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
