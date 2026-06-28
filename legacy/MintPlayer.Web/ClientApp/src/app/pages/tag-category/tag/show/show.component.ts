import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { Tag, TagService } from '@mintplayer/ng-client';
import { Subscription } from 'rxjs';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('TAG') tagInj: Tag,
    private baseUrlService: BaseUrlService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper,
  ) {
    if (serverSide === true) {
      this.setTag(tagInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTag(id);
    }
  }

  baseUrl = this.baseUrlService.getBaseUrl();
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
    this.tagService.getTag(id, true).subscribe({
      next: (tag) => {
        this.setTag(tag);
      }, error: (error) => {
        console.error('Could not get tag', error);
      }
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
    this.tagService.deleteTag(this.tag).subscribe({
      next: () => {
        this.router.navigate(['tag', 'category', this.tag.category.id]);
      }, error: (error) => {
        console.error('Could not delete tag category', error);
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
}
