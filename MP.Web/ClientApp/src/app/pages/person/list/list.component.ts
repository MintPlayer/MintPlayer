import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { PersonService } from '../../../services/person/person.service';
import { Person } from '../../../entities/person';
import { Title } from '@angular/platform-browser';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {
  constructor(@Inject('PEOPLE') private peopleInj: PaginationResponse<Person>, private personService: PersonService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    this.titleService.setTitle('People');
    if (peopleInj === null) {
      this.loadPeople();
    } else {
      this.setPersonData(peopleInj);
    }
  }

  loadPeople() {
    this.personService.pagePeople(this.tableSettings.toPaginationRequest()).then((response) => {
      this.setPersonData(response);
    }).catch((error) => {
      console.error('Could not fetch people', error);
    });
  }

  private setPersonData(data: PaginationResponse<Person>) {
    this.personData = data;
    this.tableSettings.pages.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  personData: PaginationResponse<Person> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    columns: [{
      name: 'FirstName',
      data: 'firstName',
      title: 'First Name',
      sortable: true
    }, {
      name: 'LastName',
      data: 'lastName',
      title: 'Last Name',
      sortable: true
    }, {
      name: 'Born',
      data: 'born',
      title: 'Born',
      sortable: true
    }, {
      name: 'Died',
      data: 'died',
      title: 'Died',
      sortable: true
    }],
    perPages: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    pages: {
      values: [],
      selected: 1
    },
    sortProperty: 'FirstName',
    sortDirection: 'ascending'
  });

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
