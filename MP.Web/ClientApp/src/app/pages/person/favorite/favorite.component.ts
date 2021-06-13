import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { PersonService } from '../../../services/person/person.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { Person } from '../../../entities/person';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(
    @Inject('SERVERSIDE') private serverside: boolean,
    private personService: PersonService,
    private htmlLink: HtmlLinkHelper
  ) {
    if (serverside === false) {
      this.loadFavoritePeople();
    }
  }

  loadFavoritePeople() {
    this.personService.pageFavoritePeople(this.tableSettings.toPagination()).then((response) => {
      this.setPersonData(response);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setPersonData(data: PaginationResponse<Person>) {
    this.personData = data;
    this.tableSettings.page.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  personData: PaginationResponse<Person> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    perPage: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    page: {
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
