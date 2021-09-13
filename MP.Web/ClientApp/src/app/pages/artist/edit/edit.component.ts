import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { Title } from '@angular/platform-browser';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { API_VERSION, Artist, ArtistService, MediumType, MediumTypeService, Person, SubjectService, SubjectType, Tag, TagService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject(API_VERSION) apiVersion: string,
    private artistService: ArtistService,
    private subjectService: SubjectService,
    private mediumTypeService: MediumTypeService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    this.apiVersion = apiVersion;
    if (serverSide === false) {
      // Get artist
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadArtist(id);

      // Get mediumtypes
      this.loadMediumTypes();
    }
  }

  apiVersion: string = '';
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
  };
  concurrentArtist: Artist = null;

  currentMemberSuggestions: Person[] = [];
  onProvideCurrentMemberSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [SubjectType.person]).then((people) => {
      this.currentMemberSuggestions = <Person[]>people;
    });
  }
  pastMemberSuggestions: Person[] = [];
  onProvidePastMemberSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [SubjectType.person]).then((people) => {
      this.pastMemberSuggestions = <Person[]>people;
    });
  }
  tagSuggestions: Tag[] = [];
  onProvideTagSuggestions(searchText: string) {
    this.tagService.suggestTags(searchText, true).then((tags) => {
      this.tagSuggestions = tags;
    });
  }

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
      this.oldName = artist.name;
    }
    this.artistDiffer = this.differs.find(this.artist).create();
    setTimeout(() => this.hasChanges = false);
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
      this.hasChanges = false;
      this.router.navigate(['artist', this.artist.id, this.slugifyHelper.slugify(artist.name)]);
    }).catch((error: HttpErrorResponse) => {
      switch (error.status) {
        case 409: {
          console.log("Error 409", error);
          this.concurrentArtist = error.error;
        } break;
        default: {
          console.error('Could not update artist', error);
        } break;
      }
    });
  }

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
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
