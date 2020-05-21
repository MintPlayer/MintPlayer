import { Component, OnInit, OnDestroy, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { TagCategory } from '../../../entities/tag-category';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private tagCategoryService: TagCategoryService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private differs: KeyValueDiffers) {
    if (serverSide === false) {
      // Get category
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTagCategory(id);
    }
    this.titleService.setTitle('Create tag category');
  }

  private loadTagCategory(id: number) {
    this.tagCategoryService.getTagCategory(id, true).then((tagCategory) => {
      this.setTagCategory(tagCategory);
    }).catch((error) => {
      console.log('Could not fetch tag category', error);
    });
  }

  private setTagCategory(tagCategory: TagCategory) {
    this.tagCategory = tagCategory;
    if (tagCategory !== null) {
      this.titleService.setTitle(`Edit tag category: ${tagCategory.description}`);
      this.oldDescription = tagCategory.description;
    }
    this.tagCategoryDiffer = this.differs.find(this.tagCategory).create();
    setTimeout(() => this.hasChanges = false);
  }

  oldDescription: string = '';
  tagCategory: TagCategory = {
    id: 0,
    description: '',
    color: null,
    tags: []
  };

  updateCategory() {
    this.tagCategoryService.updateTagCategory(this.tagCategory).then((category) => {
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
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
