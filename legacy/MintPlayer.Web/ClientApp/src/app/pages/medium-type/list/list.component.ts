import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';
import { MediumType, MediumTypeService, EPlayerType } from '@mintplayer/ng-client';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('MEDIUMTYPES') private mediumtypesInj: MediumType[],
    private mediumTypeService: MediumTypeService,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta,
  ) {
    this.titleService.setTitle('Medium types');
    if (serverSide === true) {
      this.setMediumTypes(mediumtypesInj);
    } else {
      this.loadMediumTypes();
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
      content: 'Here you can find a list of all the medium types in our database.'
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

  private loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).subscribe({
      next: (mediumTypes) => {
        this.setMediumTypes(mediumTypes);
      }, error: (error) => {
        console.error('Could not fetch medium types', error);
      }
    });
  }

  private setMediumTypes(data: MediumType[]) {
    this.mediumTypes = data;
  }

  public mediumTypes: MediumType[] = [];
  public playerTypes = EPlayerType;

}
