import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { PersonService } from '../../../services/person/person.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { Person } from '../../../entities/person';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') private serverside: boolean, private personService: PersonService, private htmlLink: HtmlLinkHelper) {
    if (!serverside) {
      this.loadFavoritePeople();
    }
  }

  loadFavoritePeople() {
    this.personService.pageFavoritePeople(this.tableSettings.toPaginationRequest()).then((response) => {
      this.setPersonData(response);
    }).catch((error) => {
      console.log(error);
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
