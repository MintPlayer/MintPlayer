import { Component, OnInit } from '@angular/core';
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
  constructor(private personService: PersonService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.personService.getPerson(id, true).subscribe(person => {
      this.person = person;
      this.titleService.setTitle(`Edit person: ${person.firstName} ${person.lastName}`);
      this.oldPersonName = person.firstName + " " + person.lastName;
    });
    this.mediumTypeService.getMediumTypes(false).subscribe((mediumTypes) => {
      this.mediumTypes = mediumTypes;
    });
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
