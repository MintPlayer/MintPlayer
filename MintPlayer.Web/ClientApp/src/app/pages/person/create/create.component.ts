import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpHeaders } from '@angular/common/http';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { MINTPLAYER_API_VERSION, MediumType, MediumTypeService, Person, PersonService, Tag, TagService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

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
    private personService: PersonService,
    private mediumTypeService: MediumTypeService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    this.apiVersion = apiVersion;
    this.titleService.setTitle('Create person');
    if (serverSide === false) {
      this.mediumTypeService.getMediumTypes(false).subscribe({
        next: (mediumTypes) => {
          this.mediumTypes = mediumTypes;
        }, error: (error) => {
          console.error('Could not fetch medium types', error);
        }
      });
    } else {
      this.mediumTypes = mediumTypesInj;
    }
  }

  apiVersion: string = '';
  mediumTypes: MediumType[] = [];
  person: Person = {
    id: 0,
    firstName: '',
    lastName: '',
    born: null,
    died: null,
    artists: [],
    media: [],
    tags: [],
    text: '',
    dateUpdate: null
  };

  tagSuggestions: Tag[] = [];
  onProvideTagSuggestions(searchText: string) {
    this.tagService.suggestTags(searchText, true).subscribe({
      next: (tags) => {
        this.tagSuggestions = tags;
      }
    });
  }

  savePerson() {
    this.personService.createPerson(this.person).subscribe({
      next: (person) => {
        this.hasChanges = false;
        this.router.navigate(['person', person.id, this.slugifyHelper.slugify(person.text)]);
      }, error: (error) => {
        console.error('Could not create person', error);
      }
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private personDiffer: KeyValueDiffer<string, any> = null;
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
    if (this.personDiffer !== null) {
      const changes = this.personDiffer.diff(this.person);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.personDiffer = this.differs.find(this.person).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
