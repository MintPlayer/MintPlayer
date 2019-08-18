import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Person } from '../../../interfaces/person';
import { PersonService } from '../../../services/person/person.service';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit {

  constructor(private personService: PersonService, @Inject('PERSON') private personInj: Person, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    if (personInj === null) {
      var id = parseInt(this.route.snapshot.paramMap.get("id"));
      this.personService.getPerson(id, true).subscribe(person => {
        this.setPerson(person);
      });
    } else {
      this.setPerson(personInj);
    }
  }

  private setPerson(person: Person) {
    this.person = person;
    if (person !== null) {
      this.titleService.setTitle(`${person.firstName} ${person.lastName}`);
    }
  }

  public deletePerson() {
    this.personService.deletePerson(this.person).subscribe(() => {
      this.router.navigate(["person"]);
    });
  }

  public person: Person = {
    id: 0,
    firstName: "",
    lastName: "",
    born: null,
    died: null,
    artists: [],
    media: [],
    text: ""
  };

  ngOnInit() {
  }

}
