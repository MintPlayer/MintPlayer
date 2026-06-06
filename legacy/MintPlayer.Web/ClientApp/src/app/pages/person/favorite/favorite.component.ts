import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { Person, PersonService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { Subject } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { takeUntil } from 'rxjs/operators';
import { AdvancedRouter } from '@mintplayer/ng-router';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) private serverside: boolean,
    private personService: PersonService,
    private htmlLink: HtmlLinkHelper,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
  ) {
    if (serverside === true) {
    } else {
      //this.loadFavoritePeople();
      this.route.queryParams
        .pipe(takeUntil(this.destroyed$))
        .subscribe((queryParams) => {
          this.tableSettings.perPage.selected = parseInt(queryParams['perpage'] ?? 20);
          this.tableSettings.page.selected = parseInt(queryParams['page'] ?? 1);
          this.tableSettings.sortProperty = queryParams['sortproperty'] ?? 'FirstName';
          this.tableSettings.sortDirection = queryParams['sortdirection'] ?? 'ascending';

          this.personService.pageFavoritePeople(this.tableSettings.toPagination()).subscribe({
            next: (response) => {
              this.setPersonData(response);
            }, error: (error) => {
              console.error(error);
            }
          });
        });
    }
  }

  loadFavoritePeople() {
    this.router.navigate([], {
      queryParams: {
        perpage: this.tableSettings.perPage.selected,
        page: this.tableSettings.page.selected,
        sortproperty: this.tableSettings.sortProperty,
        sortdirection: this.tableSettings.sortDirection,
      }
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

  private destroyed$ = new Subject();
  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.destroyed$.next(true);
  }

}
