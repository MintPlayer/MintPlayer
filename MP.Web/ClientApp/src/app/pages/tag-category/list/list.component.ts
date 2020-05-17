import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategory } from '../../../entities/tag-category';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {

  constructor(@Inject('TAGCATEGORIES') private tagCategoriesInj: PaginationResponse<TagCategory>, private categoryService: TagCategoryService, private router: Router, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    this.titleService.setTitle('Tag categories');
    if (tagCategoriesInj === null) {
      this.loadTagCategories();
    } else {
      this.setTagCategoryData(tagCategoriesInj);
    }
  }

  loadTagCategories() {
    this.categoryService.pageTagCategories({ perPage: this.tableSettings.perPages.selected, page: this.tableSettings.pages.selected, sortProperty: this.tableSettings.sortProperty, sortDirection: this.tableSettings.sortDirection }).then((response) => {
      this.setTagCategoryData(response);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setTagCategoryData(data: PaginationResponse<TagCategory>) {
    this.tagCategoryData = data;
    this.tableSettings.pages.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  tagCategoryData: PaginationResponse<TagCategory> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    columns: [{
      name: 'Description',
      data: 'description',
      title: 'Description',
      sortable: true
    }, {
      name: 'Color',
      data: 'color',
      title: 'Color',
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
    sortProperty: 'Description',
    sortDirection: 'ascending'
  });

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
