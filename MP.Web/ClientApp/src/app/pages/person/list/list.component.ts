import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PersonService } from '../../../services/person/person.service';
import { Person } from '../../../entities/person';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';
import { PaginationResponse } from '@mintplayer/ng-pagination';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('PEOPLE') private peopleInj: PaginationResponse<Person>,
    private personService: PersonService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta
  ) {
    this.titleService.setTitle('People');
    if (serverSide === false) {
      this.loadPeople();
    } else {
      this.setPersonData(peopleInj);
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: 'Here you can find a list of all the people in our database.'
    }]);
  }
  private addOpenGraphTags() {

  }
  private addTwitterCard() {

  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion

  loadPeople() {
    this.personService.pagePeople(this.tableSettings.toPagination()).then((response) => {
      this.setPersonData(response);
    }).catch((error) => {
      console.error('Could not fetch people', error);
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

}
