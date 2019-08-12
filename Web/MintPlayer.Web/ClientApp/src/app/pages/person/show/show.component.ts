import { Component, OnInit } from '@angular/core';
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

  constructor(private personService: PersonService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.personService.getPerson(id, true).subscribe(person => {
      this.person = person;
      if (person != null) {
        this.titleService.setTitle(person.firstName + ' ' + person.lastName);
      }
    });
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
