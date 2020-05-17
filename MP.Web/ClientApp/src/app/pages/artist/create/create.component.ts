import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Artist } from '../../../entities/artist';
import { Person } from '../../../entities/person';
import { ArtistService } from '../../../services/artist/artist.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../entities/medium-type';
import { Title } from '@angular/platform-browser';
import { Tag } from '../../../entities/tag';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {
  constructor(@Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[], private artistService: ArtistService, private mediumTypeService: MediumTypeService, private router: Router, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    this.titleService.setTitle('Create artist');
    if (mediumTypesInj === null) {
      this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }).catch((error) => {
        console.error('Could not fetch medium types', error);
      });
    } else {
      this.mediumTypes = mediumTypesInj;
    }
  }

  mediumTypes: MediumType[] = [];
  artist: Artist = {
    id: 0,
    name: '',
    yearStarted: null,
    yearQuit: null,
    currentMembers: [],
    pastMembers: [],
    songs: [],
    media: [],
    tags: [],
    text: '',
    dateUpdate: null
  };

  saveArtist() {
    this.artistService.createArtist(this.artist).then((artist) => {
      this.router.navigate(['/artist', artist.id, this.slugifyHelper.slugify(artist.name)]);
    }).catch((error) => {
      console.error('Could not create artist', error);
    });
  }

  public httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true),
    'Content-Type': 'application/json'
  });

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
