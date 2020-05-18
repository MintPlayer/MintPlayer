import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { PersonService } from '../../../services/person/person.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { Person } from '../../../entities/person';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { UrlGenerator } from '../../../helpers/url-generator.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('PERSON') personInj: Person,
    @Inject('BASE_URL') private baseUrl: string,
    private personService: PersonService,
    private router: Router,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper,
    private urlGenerator: UrlGenerator
  ) {
    if (serverSide === true) {
      this.setPerson(personInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPerson(id);
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
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
      content: `Details for ${this.person.text}`
    }]);
  }
  private addOpenGraphTags() {
    this.ogMetaTags = this.metaService.addTags([{
      property: 'og:type',
      content: 'profile'
    }, {
      property: 'og:url',
      content: this.urlGenerator.generateFullUrl(this.person)
    }, {
      property: 'profile:first_name',
      content: this.person.firstName
    }, {
      property: 'profile:last_name',
      content: this.person.lastName
    }, {
      property: 'og:title',
      content: this.person.text
    }, {
      property: 'og:description',
      content: `Information for ${this.person.text}`
    }, {
      property: 'og:updated_time',
      content: new Date(this.person.dateUpdate).toISOString()
    }]);
  }
  private addTwitterCard() {
    this.twitterMetaTags = this.metaService.addTags([{
      property: 'twitter:card',
      content: 'summary'
    }, {
      property: 'twitter:url',
      content: this.urlGenerator.generateFullUrl(this.person)
    }, {
      property: 'twitter:image',
      content: `${this.baseUrl}/assets/logo/music_note_72.png`
    }, {
      property: 'twitter:title',
      content: this.person.text
    }, {
      property: 'twitter:description',
      content: `Information for ${this.person.text}`
    }]);
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

  private loadPerson(id: number) {
    this.personService.getPerson(id, true).then((person) => {
      this.setPerson(person);
    }).catch((error) => {
      console.error('Could not get person', error);
    });
  }

  private setPerson(person: Person) {
    this.person = person;
    this.removeMetaTags();

    if (this.person != null) {
      this.titleService.setTitle(`${this.person.firstName} ${this.person.lastName}`);
      this.addMetaTags();
    }
  }

  public deletePerson() {
    this.personService.deletePerson(this.person).then(() => {
      this.router.navigate(['person']);
    }).catch((error) => {
      console.error('Could not delete person', error);
    });
  }

  public person: Person = {
    id: 0,
    firstName: '',
    lastName: '',
    born: null,
    died: null,
    artists: [],
    media: [],
    tags: [],
    text: '',
    dateUpdate: null
  };
}
