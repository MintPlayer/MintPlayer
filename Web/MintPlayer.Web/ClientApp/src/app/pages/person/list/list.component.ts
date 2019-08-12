import { Component, OnInit } from '@angular/core';
import { PersonService } from '../../../services/person/person.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Person } from '../../../interfaces/person';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  constructor(private personService: PersonService, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    this.titleService.setTitle('People');
    this.loadPeople();
  }

  people: Person[] = [];

  private loadPeople() {
    this.personService.getPeople(false).subscribe(people => {
      this.people = people;
    });
  }

  ngOnInit() {
  }
}
