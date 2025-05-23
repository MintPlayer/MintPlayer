import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { AccountService, BlogPost, BlogPostService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { UrlGenerator } from '../../../../helpers/url-generator.helper';
import { WordCountPipe } from '../../../../pipes/word-count/word-count.pipe';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('BLOGPOST') private blogPostInj: BlogPost,
    private baseUrlService: BaseUrlService,
    private blogPostService: BlogPostService,
    private accountService: AccountService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
    private htmlLink: HtmlLinkHelper,
    private urlGenerator: UrlGenerator,
    private wordCountPipe: WordCountPipe,
  ) {
    if (serverSide === true) {
      this.setBlogPost(blogPostInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadBlogPost(id);
      this.accountService.currentRoles().subscribe({
        next: (roles) => {
          this.isBlogger = roles.indexOf('Blogger') > -1;
        }
      });
    }
  }

  baseUrl = this.baseUrlService.getBaseUrl();
  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
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
      content: this.blogPost.headline
    }]);
  }
  private addOpenGraphTags() {
    this.ogMetaTags = this.metaService.addTags([{
      property: 'og:type',
      content: 'article'
    }, {
      property: 'og:url',
      content: this.urlGenerator.generateFullUrl(this.blogPost)
    }, {
      property: 'og:title',
      content: this.blogPost.title
    }, {
      property: 'og:description',
      content: this.blogPost.headline
    }, {
      property: 'og:updated_time',
      content: new Date(this.blogPost.published).toISOString()
    }]);
  }
  private addTwitterCard() {
    this.twitterMetaTags = this.metaService.addTags([{
      property: 'twitter:card',
      content: 'summary'
    }, {
      property: 'twitter:url',
      content: this.urlGenerator.generateFullUrl(this.blogPost)
    }, {
      property: 'twitter:image',
      content: `${this.baseUrl}/assets/logo/music_note_72.png`
    }, {
      property: 'twitter:title',
      content: this.blogPost.title
    }, {
      property: 'twitter:description',
      content: this.blogPost.headline
    }]);
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

  blogPost: BlogPost = {
    id: null,
    title: '',
    headline: '',
    body: '',
    author: {
      id: null,
      email: '',
      userName: '',
      isTwoFactorEnabled: false,
      bypass2faForExternalLogin: false,
      pictureUrl: ''
    },
    published: null
  };
  isBlogger: boolean = false;

  blogPostLd: {
    '@context': 'http://schema.org',
    '@type': 'BlogPosting',
    'url': string,
    'headline': string,
    'wordcount': string,
    'publisher': string,
    'datePublished': string,
    'description': string,
    'articleBody': string,
    'author': {
      '@context': 'http://schema.org',
      '@type': 'Person',
      'name': string
    }
  } = {
      '@context': 'http://schema.org',
      '@type': 'BlogPosting',
      'url': '',
      'headline': '',
      'wordcount': '',
      'publisher': '',
      'datePublished': '',
      'description': '',
      'articleBody': '',
      'author': {
        '@context': 'http://schema.org',
        '@type': 'Person',
        'name': ''
      }
    };

  private loadBlogPost(id: number) {
    this.blogPostService.getBlogPost(id).subscribe({
      next: (blogPost) => {
        this.setBlogPost(blogPost);
      }, error: (error) => {
        console.error('Could not get blogpost', error);
      }
    });
  }

  private setBlogPost(blogPost: BlogPost) {
    this.blogPost = blogPost;
    this.removeMetaTags();

    if (blogPost != null) {
      //#region Title
      this.titleService.setTitle(blogPost.title);
      //#endregion
      //#region LD+json
      this.blogPostLd = {
        '@context': 'http://schema.org',
        '@type': 'BlogPosting',
        'url': this.urlGenerator.generateFullUrl(blogPost),
        'headline': blogPost.title,
        'description': blogPost.headline,
        'publisher': 'MintPlayer',
        'datePublished': new Date(blogPost.published).toISOString(),
        'author': {
          '@context': 'http://schema.org',
          '@type': 'Person',
          'name': blogPost.author.userName
        },
        'articleBody': blogPost.body,
        'wordcount': String(this.wordCountPipe.transform(blogPost.body))
      };
      //#endregion
      this.addMetaTags();
    }
  }

  public deleteBlogPost() {
    this.blogPostService.deleteBlogPost(this.blogPost).subscribe({
      next: () => {
        this.router.navigate(['/community', 'blog']);
      }, error: (error) => {
        console.error('Could not delete blog post', error);
      }
    });
  }
}
