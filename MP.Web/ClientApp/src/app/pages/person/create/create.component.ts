import { Component, OnInit, Inject, OnDestroy, HostListener } from '@angular/core';
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

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {

  constructor(@Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[], private personService: PersonService, private mediumTypeService: MediumTypeService, private router: Router, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    this.titleService.setTitle('Create person');
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
      this.router.navigate(['person', person.id, this.slugifyHelper.slugify(person.text)]);
    }).catch((error) => {
      console.error('Could not create person', error);
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
