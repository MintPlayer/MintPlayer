import { Component, OnInit } from '@angular/core';
import { PersonService } from '../../../services/person/person.service';
import { Person } from '../../../interfaces/person';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../interfaces/medium-type';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  constructor(private personService: PersonService, private mediumTypeService: MediumTypeService, private router: Router, private titleService: Title) {
    this.titleService.setTitle('Create person');
    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
  }

  mediumTypes: MediumType[] = [];
  person: Person = {
    id: 0,
    firstName: "",
    lastName: "",
    born: null,
    died: null,
    artists: [],
    media: [],
    text: ""
  };

  savePerson() {
    this.personService.createPerson(this.person).subscribe((person) => {
      this.router.navigate(["person", person.id]);
    });
  }

  ngOnInit() {
  }

}
