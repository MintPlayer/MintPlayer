import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { TagCategory } from '../../../entities/tag-category';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private tagCategoryService: TagCategoryService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
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
    this.titleService.setTitle(`Edit tag category: ${tagCategory.description}`);
    this.oldDescription = tagCategory.description;
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
