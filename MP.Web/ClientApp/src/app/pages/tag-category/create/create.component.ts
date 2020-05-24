import { Component, OnInit, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategory } from '../../../entities/tag-category';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { ExtendedRouter } from '../../../helpers/extended-router';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(private tagCategoryService: TagCategoryService, private router: ExtendedRouter, private titleService: Title, private htmlLink: HtmlLinkHelper, private differs: KeyValueDiffers) {
    this.titleService.setTitle('Create tag category');
  }

  tagCategory: TagCategory = {
    id: 0,
    description: '',
    color: null,
    tags: []
  };

  public saveCategory() {
    this.tagCategoryService.createTagCategory(this.tagCategory).then((category) => {
      this.router.navigate(['tag', 'category', category.id]);
    }).catch((error) => {
      console.log('Could not create tag category', error);
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private tagCategoryDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.tagCategoryDiffer !== null) {
      const changes = this.tagCategoryDiffer.diff(this.tagCategory);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.tagCategoryDiffer = this.differs.find(this.tagCategory).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
