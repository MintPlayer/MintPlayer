import { Component, OnInit, Inject, OnDestroy, HostListener } from '@angular/core';
import { TagService } from '../../../../services/tag/tag.service';
import { TagCategoryService } from '../../../../services/tag-category/tag-category.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Tag } from '../../../../entities/tag';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { TagCategory } from '../../../../entities/tag-category';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('TAGCATEGORY') tagCategoryInj: TagCategory, @Inject('BASE_URL') private baseUrl: string, private tagCategoryService: TagCategoryService, private tagService: TagService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
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
      var categoryId = parseInt(this.route.snapshot.paramMap.get('category_id'));
      this.router.navigate(['/tag', 'category', categoryId, 'tags', tag.id]);
    }).catch((error) => {
      console.log(error);
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }
  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
