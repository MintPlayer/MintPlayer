import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { Tag } from '../../../../entities/tag';
import { TagService } from '../../../../services/tag/tag.service';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { NavigationHelper } from '../../../../helpers/navigation.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('TAG') tagInj: Tag,
    @Inject('BASE_URL') private baseUrl: string,
    private tagService: TagService,
    private navigation: NavigationHelper,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper
  ) {
    if (serverSide === true) {
      this.setTag(tagInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTag(id);
    }
  }

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

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: `The tag ${this.tag.description}.`
    }]);
  }
  private addOpenGraphTags() {
  }
  private addTwitterCard() {
  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion

  private loadTag(id: number) {
    this.tagService.getTag(id, true).then((tag) => {
      this.setTag(tag);
    }).catch((error) => {
      console.log('Could not get tag', error);
    });
  }

  private setTag(tag: Tag) {
    this.tag = tag;
    this.removeMetaTags();

    if (this.tag != null) {
      this.titleService.setTitle(this.tag.description);
      this.addMetaTags();
    }
  }

  deleteTag() {
    this.tagService.deleteTag(this.tag).then(() => {
      this.navigation.navigate(['tag', 'category', this.tag.category.id]);
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
}
