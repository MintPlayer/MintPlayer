import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
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
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy, DoCheck, HasChanges {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject(MINTPLAYER_API_VERSION) apiVersion: string,
    private personService: PersonService,
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
      // Get person
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPerson(id);

      // Get mediumtypes
      this.loadMediumTypes();
    }
  }

  private loadPerson(id: number) {
    this.personService.getPerson(id, true).subscribe({
      next: (person) => {
        this.setPerson(person);
      }, error: (error) => {
        console.error('Could not fetch person', error);
      }
    });
  }

  private setPerson(person: Person) {
    this.person = person;
    if (person !== null) {
      this.titleService.setTitle(`Edit person: ${person.firstName} ${person.lastName}`);
      this.oldPersonName = `${person.firstName} ${person.lastName}`;
    }
    this.personDiffer = this.differs.find(this.person).create();
    setTimeout(() => this.hasChanges = false);
  }

  private loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).subscribe({
      next: (mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }, error: (error) => {
        console.error('Could not get medium types', error);
      }
    });
  }

  apiVersion: string = '';
  oldPersonName: string = '';
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
  concurrentPerson: Person = null;

  tagSuggestions: Tag[] = [];
  onProvideTagSuggestions(searchText: string) {
    this.tagService.suggestTags(searchText, true).subscribe({
      next: (tags) => {
        this.tagSuggestions = tags;
      }
    });
  }

  updatePerson() {
    this.personService.updatePerson(this.person).subscribe({
      next: (person) => {
        this.hasChanges = false;
        this.router.navigate(['person', this.person.id, this.slugifyHelper.slugify(person.text)]);
      }, error: (error) => {
        switch (error.status) {
          case 409: {
            console.log("Error 409", error);
            this.concurrentPerson = error.error;
          } break;
          default: {
            console.error('Could not update person', error);
          } break;
        }
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
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
