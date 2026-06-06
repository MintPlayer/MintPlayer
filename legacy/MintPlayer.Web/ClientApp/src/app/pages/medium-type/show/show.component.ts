import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { MediumType, MediumTypeService, EPlayerType } from '@mintplayer/ng-client';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {
  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('MEDIUMTYPE') private mediumTypeInj: MediumType,
    private mediumTypeService: MediumTypeService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper,
  ) {
    if (serverSide === true) {
      this.setMediumType(mediumTypeInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadMediumType(id);
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
      content: `The medium type ${this.mediumType.description}`
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

  private loadMediumType(id: number) {
    this.mediumTypeService.getMediumType(id, true).subscribe({
      next: (mediumtype) => {
        this.setMediumType(mediumtype);
      }, error: (error) => {
        console.error('Could not fetch medium type', error);
      }
    });
  }

  private setMediumType(mediumType: MediumType) {
    this.mediumType = mediumType;
    this.removeMetaTags();

    if (mediumType != null) {
      //#region Title
      this.titleService.setTitle(`Medium type: ${mediumType.description}`);
      //#endregion
      this.addMetaTags();
    }
  }

  public deleteMediumType() {
    this.mediumTypeService.deleteMediumType(this.mediumType).subscribe({
      next: () => {
        this.router.navigate(['mediumtype']);
      }, error: (error) => {
        console.error('Could not delete medium type', error);
      }
    });
  }

  public playerTypeEnum = EPlayerType;
  public mediumType: MediumType = {
    id: 0,
    description: '',
    visible: true,
  };
}
