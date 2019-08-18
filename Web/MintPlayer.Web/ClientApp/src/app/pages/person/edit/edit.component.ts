import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PersonService } from '../../../services/person/person.service';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Person } from '../../../interfaces/person';
import { MediumType } from '../../../interfaces/medium-type';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {
  constructor(private personService: PersonService, @Inject('PERSON') private personInj: Person, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    if (personInj === null) {
      var id = parseInt(this.route.snapshot.paramMap.get("id"));
      this.personService.getPerson(id, true).subscribe(person => {
        this.setPerson(person);
      });
    } else {
      this.setPerson(personInj);
    }
    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
  }

  private setPerson(person: Person) {
    this.person = person;
    if (person !== null) {
      this.titleService.setTitle(`Edit person: ${person.firstName} ${person.lastName}`);
      this.oldPersonName = person.firstName + " " + person.lastName;
    }
  }

  ngOnInit() {
  }

  mediumTypes: MediumType[] = [];
  oldPersonName: string = "";
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

  updatePerson() {
    this.personService.updatePerson(this.person).subscribe(() => {
      this.router.navigate(["person", this.person.id]);
    });
  }
}
