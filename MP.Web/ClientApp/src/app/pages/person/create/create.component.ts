import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PersonService } from '../../../services/person/person.service';
import { Person } from '../../../entities/person';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../entities/medium-type';
import { Tag } from '../../../entities/tag';
import { HttpHeaders } from '@angular/common/http';
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
    @Inject('SERVERSIDE') private serverSide: boolean,
    @Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[],
    private personService: PersonService,
    private mediumTypeService: MediumTypeService,
    private router: Router,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers
  ) {
    this.titleService.setTitle('Create person');
    if (serverSide === false) {
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

  savePerson() {
    this.personService.createPerson(this.person).then((person) => {
      this.hasChanges = false;
      this.router.navigate(['person', person.id, this.slugifyHelper.slugify(person.text)]);
    }).catch((error) => {
      console.error('Could not create person', error);
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
