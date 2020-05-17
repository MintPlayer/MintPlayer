import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Tag } from '../../../../entities/tag';
import { TagService } from '../../../../services/tag/tag.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('TAG') tagInj: Tag, @Inject('BASE_URL') private baseUrl: string, private tagService: TagService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    if (serverSide === true) {
      this.setTag(tagInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTag(id);
    }
  }

  private loadTag(id: number) {
    this.tagService.getTag(id, true).then((tag) => {
      this.setTag(tag);
    }).catch((error) => {
      console.log('Could not get tag', error);
    });
  }

  private setTag(tag: Tag) {
    this.tag = tag;
    if (this.tag != null) {
      this.titleService.setTitle(this.tag.description);
    }
  }

  deleteTag() {
    this.tagService.deleteTag(this.tag).then(() => {
      this.router.navigate(['tag', 'category', this.tag.category.id]);
    }).catch((error) => {
      console.error('Could not delete tag category', error);
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

  private routeParamsSubscription: Subscription;
  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.routeParamsSubscription = this.route.params.subscribe((routeParams) => {
      this.loadTag(routeParams.id);
    });
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    if (this.routeParamsSubscription !== null) {
      this.routeParamsSubscription.unsubscribe();
    }
  }
}
