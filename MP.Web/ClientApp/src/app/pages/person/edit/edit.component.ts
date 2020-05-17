import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
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

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {
  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private personService: PersonService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    if (serverSide === false) {
      // Get person
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.personService.getPerson(id, true).then((person) => {
        this.person = person;
        this.titleService.setTitle(`Edit person: ${person.firstName} ${person.lastName}`);
        this.oldPersonName = `${person.firstName} ${person.lastName}`;
      }).catch((error) => {
        console.error('Could not fetch person', error);
      });

      // Get mediumtypes
      this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }).catch((error) => {
        console.error('Could not fetch medium types', error);
      });
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
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
}
