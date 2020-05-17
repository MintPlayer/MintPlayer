import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
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

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, @Inject('BLOGPOSTS') private blogPostsInj: BlogPost[], private blogPostService: BlogPostService, private accountService: AccountService, private titleService: Title, private htmlLink: HtmlLinkHelper) {
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
