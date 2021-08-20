import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { TagCategory, TagCategoryService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('TAGCATEGORY') tagCategoryInj: TagCategory,
    @Inject(BASE_URL) private baseUrl: string,
    private tagCategoryService: TagCategoryService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper
  ) {
    if (serverSide === true) {
      this.setTagCategory(tagCategoryInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTagCategory(id);
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
      content: `The tag category ${this.tagCategory.description}.`
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

  private loadTagCategory(id: number) {
    this.tagCategoryService.getTagCategory(id, true).then((tagCategory) => {
      this.setTagCategory(tagCategory);
    }).catch((error) => {
      console.error('Could not get tag category', error);
    });
  }

  private setTagCategory(tagCategory: TagCategory) {
    this.tagCategory = tagCategory;
    this.removeMetaTags();

    if (this.tagCategory != null) {
      this.titleService.setTitle(this.tagCategory.description);
      this.addMetaTags();
    }
  }

  deleteTagCategory() {
    this.tagCategoryService.deleteTagCategory(this.tagCategory).then(() => {
      this.router.navigate(['tag', 'category']);
    }).catch((error) => {
      console.error('Could not delete tag category', error);
    });
  }

  tagCategory: TagCategory = {
    id: 0,
    description: '',
    color: '#888',
    tags: []
  };
}
