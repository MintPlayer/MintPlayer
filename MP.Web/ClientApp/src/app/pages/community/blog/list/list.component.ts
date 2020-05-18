import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';
import { BlogPost } from '../../../../entities/blog-post';
import { BlogPostService } from '../../../../services/blog-post/blog-post.service';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { AccountService } from '../../../../services/account/account.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {

  constructor(
    @Inject('SERVERSIDE') private serverSide: boolean,
    @Inject('BLOGPOSTS') private blogPostsInj: BlogPost[],
    private blogPostService: BlogPostService,
    private accountService: AccountService,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta
  ) {
    this.titleService.setTitle('MintPlayer blog');
    if (this.serverSide) {
      this.setBlogPosts(blogPostsInj);
    } else {
      this.loadBlogPosts();
      this.accountService.currentRoles().then((roles) => {
        this.isBlogger = roles.indexOf('Blogger') > -1;
      });
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
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
      content: 'Welcome to our blog. This is the place where we keep you up-to-date with the evolution of our platform.'
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

  private loadBlogPosts() {
    this.blogPostService.getBlogPosts().then((blogPosts) => {
      this.setBlogPosts(blogPosts);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setBlogPosts(data: BlogPost[]) {
    this.blogPosts = data;
  }

  blogPosts: BlogPost[] = [];
  isBlogger: boolean = false;

}
