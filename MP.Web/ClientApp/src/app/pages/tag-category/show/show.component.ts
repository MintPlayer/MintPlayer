import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { TagCategory } from '../../../entities/tag-category';
import { TagCategoryService } from '../../../services/tag-category/tag-category.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('TAGCATEGORY') tagCategoryInj: TagCategory, @Inject('BASE_URL') private baseUrl: string, private tagCategoryService: TagCategoryService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    if (serverSide === true) {
      this.setTagCategory(tagCategoryInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTagCategory(id);
    }
  }

  private loadTagCategory(id: number) {
    this.tagCategoryService.getTagCategory(id, true).then((tagCategory) => {
      this.setTagCategory(tagCategory);
    }).catch((error) => {
      console.log('Could not get tag category', error);
    });
  }

  private setTagCategory(tagCategory: TagCategory) {
    this.tagCategory = tagCategory;
    if (this.tagCategory != null) {
      this.titleService.setTitle(this.tagCategory.description);
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
