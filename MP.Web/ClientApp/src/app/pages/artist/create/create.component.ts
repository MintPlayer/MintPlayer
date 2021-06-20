import { Component, OnInit, Inject, OnDestroy, HostListener, KeyValueDiffers, KeyValueDiffer, DoCheck } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpHeaders } from '@angular/common/http';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { Artist } from '../../../entities/artist';
import { ArtistService } from '../../../services/artist/artist.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../entities/medium-type';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { HasChanges } from '../../../interfaces/has-changes';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[],
    private artistService: ArtistService,
    private mediumTypeService: MediumTypeService,
    private router: AdvancedRouter,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers
  ) {
    this.titleService.setTitle('Create artist');
    if (serverSide === true) {
      this.mediumTypes = mediumTypesInj;
    } else {
      this.loadMediumTypes();
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

  loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    }).catch((error) => {
      console.error('Could not fetch medium types', error);
    });
  }

  saveArtist() {
    this.artistService.createArtist(this.artist).then((artist) => {
      this.hasChanges = false;
      this.router.navigate(['/artist', artist.id, this.slugifyHelper.slugify(artist.name)]);
    }).catch((error) => {
      console.error('Could not create artist', error);
    });
  }

  public httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true),
    'Content-Type': 'application/json'
  });

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private artistDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.artistDiffer !== null) {
      const changes = this.artistDiffer.diff(this.artist);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.artistDiffer = this.differs.find(this.artist).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
