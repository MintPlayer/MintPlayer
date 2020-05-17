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

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('PERSON') personInj: Person, @Inject('BASE_URL') private baseUrl: string, private personService: PersonService, private router: Router, private route: ActivatedRoute, private titleService: Title, private metaService: Meta, private htmlLink: HtmlLinkHelper, private urlGenerator: UrlGenerator) {
    if (serverSide === true) {
      this.setPerson(personInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadPerson(id);
    }
  }

  private loadPerson(id: number) {
    this.personService.getPerson(id, true).then((person) => {
      this.setPerson(person);
    }).catch((error) => {
      console.error('Could not get person', error);
    });
  }

  private metaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private setPerson(person: Person) {
    this.person = person;

    // Remove the existing meta-tags on the page
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });

    if (this.person != null) {
      this.titleService.setTitle(`${this.person.firstName} ${this.person.lastName}`);
      this.metaTags = this.metaService.addTags([{
        name: 'description',
        content: `Details for ${person.text}`
      }]);
      this.ogMetaTags = this.metaService.addTags([{
        property: 'og:type',
        content: 'profile'
      }, {
        property: 'og:url',
        content: this.urlGenerator.generateFullUrl(person)
      }, {
        property: 'profile:first_name',
        content: person.firstName
      }, {
        property: 'profile:last_name',
        content: person.lastName
      }, {
        property: 'og:title',
        content: person.text
      }, {
        property: 'og:description',
        content: `Information for ${person.text}`
      }, {
        property: 'og:updated_time',
        content: new Date(person.dateUpdate).toISOString()
      }]);
      this.twitterMetaTags = this.metaService.addTags([{
        property: 'twitter:card',
        content: 'summary'
      }, {
        property: 'twitter:url',
        content: this.urlGenerator.generateFullUrl(person)
      }, {
        property: 'twitter:image',
        content: `${this.baseUrl}/assets/logo/music_note_72.png`
      }, {
        property: 'twitter:title',
        content: person.text
      }, {
        property: 'twitter:description',
        content: `Information for ${person.text}`
      }]);
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.metaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.ogMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
    this.twitterMetaTags.forEach((tag) => {
      this.metaService.removeTagElement(tag);
    });
  }
}
