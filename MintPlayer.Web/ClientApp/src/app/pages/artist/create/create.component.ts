import { Component, OnInit, Inject, OnDestroy, HostListener, KeyValueDiffers, KeyValueDiffer, DoCheck } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpHeaders } from '@angular/common/http';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { MINTPLAYER_API_VERSION, Artist, ArtistService, MediumType, MediumTypeService, Person, PersonService, SubjectService, ESubjectType, Tag, TagService } from '@mintplayer/ng-client';
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
    @Inject(MINTPLAYER_API_VERSION) apiVersion: string,
    @Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[],
    private artistService: ArtistService,
    private subjectService: SubjectService,
    private mediumTypeService: MediumTypeService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    this.apiVersion = apiVersion;
    this.titleService.setTitle('Create artist');
    if (serverSide === true) {
      this.mediumTypes = mediumTypesInj;
    } else {
      this.loadMediumTypes();
    }
  }

  apiVersion: string = '';
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

  currentMemberSuggestions: Person[] = [];
  onProvideCurrentMemberSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [ESubjectType.person]).subscribe({
      next: (people) => {
        this.currentMemberSuggestions = <Person[]>people;
      }
    });
  }
  pastMemberSuggestions: Person[] = [];
  onProvidePastMemberSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [ESubjectType.person]).subscribe({
      next: (people) => {
        this.pastMemberSuggestions = <Person[]>people;
      }
    });
  }
  tagSuggestions: Tag[] = [];
  onProvideTagSuggestions(searchText: string) {
    this.tagService.suggestTags(searchText, true).subscribe({
      next: (tags) => {
        this.tagSuggestions = tags;
      }
    });
  }

  loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).subscribe({
      next: (mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }, error: (error) => {
        console.error('Could not fetch medium types', error);
      }
    });
  }

  saveArtist() {
    this.artistService.createArtist(this.artist).subscribe({
      next: (artist) => {
        this.hasChanges = false;
        this.router.navigate(['/artist', artist.id, this.slugifyHelper.slugify(artist.name)]);
      }, error: (error) => {
        console.error('Could not create artist', error);
      }
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
