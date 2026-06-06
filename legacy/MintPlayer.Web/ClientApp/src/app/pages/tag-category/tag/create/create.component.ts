import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { Tag, TagCategory, TagCategoryService, TagService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('TAGCATEGORY') tagCategoryInj: TagCategory,
    private tagCategoryService: TagCategoryService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private differs: KeyValueDiffers,
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
    this.tagCategoryService.getTagCategory(categoryId, false).subscribe({
      next: (category) => {
        this.tag.category = category;
      }, error: (error) => {
        console.error(error);
      }
    });
  }

  loadParentTag(parentTagId: number) {
    this.tagService.getTag(parentTagId, true).subscribe({
      next: (parent) => {
        this.tag.parent = parent;
      }, error: (error) => {
        console.error(error);
      }
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
    this.tagService.createTag(this.tag).subscribe({
      next: (tag) => {
        this.hasChanges = false;
        var categoryId = parseInt(this.route.snapshot.paramMap.get('category_id'));
        this.router.navigate(['/tag', 'category', categoryId, 'tags', tag.id]);
      }, error: (error) => {
        console.error(error);
      }
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
