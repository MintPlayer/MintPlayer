import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { TagCategory, TagCategoryService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('TAGCATEGORIES') private tagCategoriesInj: PaginationResponse<TagCategory>,
    private categoryService: TagCategoryService,
    private router: AdvancedRouter,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper
  ) {
    this.titleService.setTitle('Tag categories');
    if (serverSide === true) {
      this.setTagCategoryData(tagCategoriesInj);
    } else {
      this.loadTagCategories();
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
      content: 'Here you can find a list of all the tag categories in our database.'
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

  loadTagCategories() {
    this.categoryService.pageTagCategories({ perPage: this.tableSettings.perPage.selected, page: this.tableSettings.page.selected, sortProperty: this.tableSettings.sortProperty, sortDirection: this.tableSettings.sortDirection }).then((response) => {
      this.setTagCategoryData(response);
    }).catch((error) => {
      console.error(error);
    });
  }

  private setTagCategoryData(data: PaginationResponse<TagCategory>) {
    this.tagCategoryData = data;
    this.tableSettings.page.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  tagCategoryData: PaginationResponse<TagCategory> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    perPage: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    page: {
      values: [],
      selected: 1
    },
    sortProperty: 'Description',
    sortDirection: 'ascending'
  });

}
