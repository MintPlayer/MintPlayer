import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ArtistService } from '../../../services/artist/artist.service';
import { Artist } from '../../../entities/artist';
import { Person } from '../../../entities/person';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../entities/medium-type';
import { Tag } from '../../../entities/tag';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private artistService: ArtistService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    if (serverSide === false) {
      // Get artist
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadArtist(id);

      // Get mediumtypes
      this.loadMediumTypes();
    }
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
    tags: [],
    text: '',
    dateUpdate: null
  }

  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  private loadArtist(id: number) {
    this.artistService.getArtist(id, true).then((artist) => {
      this.setArtist(artist);
    }).catch((error) => {
      console.error('Could not fetch artist', error);
    });
  }

  private setArtist(artist: Artist) {
    this.artist = artist;
    if (artist !== null) {
      this.titleService.setTitle(`Edit artist: ${artist.name}`);
    }
    this.oldName = artist.name;
  }

  private loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    }).catch((error) => {
      console.error('Could not get medium types', error);
    });
  }

  public updateArtist() {
    this.artistService.updateArtist(this.artist).then((artist) => {
      this.router.navigate(['artist', this.artist.id, this.slugifyHelper.slugify(artist.name)]);
    }).catch((error) => {
      console.error('Could not update artist', error);
    });
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
