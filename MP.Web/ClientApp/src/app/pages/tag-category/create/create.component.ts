import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategory } from '../../../entities/tag-category';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {

  constructor(private tagCategoryService: TagCategoryService, private router: Router, private titleService: Title, private htmlLink: HtmlLinkHelper) {
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
