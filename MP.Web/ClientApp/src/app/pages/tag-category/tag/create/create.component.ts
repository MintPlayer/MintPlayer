import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { TagService } from '../../../../services/tag/tag.service';
import { TagCategoryService } from '../../../../services/tag-category/tag-category.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Tag } from '../../../../entities/tag';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { TagCategory } from '../../../../entities/tag-category';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';
import { NavigationHelper } from '../../../../helpers/navigation.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('TAGCATEGORY') tagCategoryInj: TagCategory,
    private tagCategoryService: TagCategoryService,
    private tagService: TagService,
    private navigation: NavigationHelper,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private differs: KeyValueDiffers
  ) {
    if (serverSide === true) {
      this.tag.category = tagCategoryInj;
    } else {
      var parentTagId = this.route.snapshot.paramMap.has('id')
        ? parseInt(this.route.snapshot.paramMap.get('id'))
        : null;
      var categoryId = parseInt(this.route.snapshot.paramMap.get('category_id'));

      if (parentTagId === null) {
      } else {
        this.loadParentTag(parentTagId);
      }
      this.loadTagCategory(categoryId);
    }
  }

  loadTagCategory(categoryId: number) {
    this.tagCategoryService.getTagCategory(categoryId, false).then((category) => {
      this.tag.category = category;
    }).catch((error) => {
      console.log(error);
    });
  }

  loadParentTag(parentTagId: number) {
    this.tagService.getTag(parentTagId, true).then((parent) => {
      this.tag.parent = parent;
    }).catch((error) => {
      console.error(error);
    });
  }

  tag: Tag = {
    id: 0,
    description: '',
    category: {
      id: 0,
      description: '',
      color: null,
      tags: []
    },
    subjects: [],
    parent: null,
    children: []
  };

  saveTag() {
    this.tagService.createTag(this.tag).then((tag) => {
      this.hasChanges = false;
      var categoryId = parseInt(this.route.snapshot.paramMap.get('category_id'));
      this.navigation.navigate(['/tag', 'category', categoryId, 'tags', tag.id]);
    }).catch((error) => {
      console.log(error);
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private tagDiffer: KeyValueDiffer<string, any> = null;
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
    if (this.tagDiffer !== null) {
      const changes = this.tagDiffer.diff(this.tag);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.tagDiffer = this.differs.find(this.tag).create();
    setTimeout(() => this.hasChanges = false);
  }
  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
