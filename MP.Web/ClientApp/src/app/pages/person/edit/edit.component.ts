import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { PersonService } from '../../../services/person/person.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Person } from '../../../entities/person';
import { MediumType } from '../../../entities/medium-type';
import { Tag } from '../../../entities/tag';
import { HttpHeaders } from '@angular/common/http';
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
  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private personService: PersonService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper, private differs: KeyValueDiffers) {
    if (serverSide === false) {
      // Get person
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPerson(id);

      // Get mediumtypes
      this.loadMediumTypes();
    }
  }

  private loadPerson(id: number) {
    this.personService.getPerson(id, true).then((person) => {
      this.setPerson(person);
    }).catch((error) => {
      console.error('Could not fetch person', error);
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
    this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    }).catch((error) => {
      console.error('Could not get medium types', error);
    });
  }

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

  public httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  updatePerson() {
    this.personService.updatePerson(this.person).then((person) => {
      this.router.navigate(['person', this.person.id, this.slugifyHelper.slugify(person.text)]);
    }).catch((error) => {
      console.error('Could not update person', error);
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
