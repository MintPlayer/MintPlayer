import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { PersonService } from '../../../services/person/person.service';
import { Person } from '../../../entities/person';
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
  constructor(
    @Inject('SERVERSIDE') private serverSide: boolean,
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

}
