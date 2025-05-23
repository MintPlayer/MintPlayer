import { Component, OnInit, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, ActivatedRoute } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { BlogPost, BlogPostService } from '@mintplayer/ng-client';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, DoCheck, HasChanges {

  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    private blogPostService: BlogPostService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private slugifyPipe: SlugifyPipe,
    private differs: KeyValueDiffers,
  ) {
    if (serverSide === false) {
      // Get blogpost
      let id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadBlogPost(id);
    }
  }

  private loadBlogPost(id: number) {
    this.blogPostService.getBlogPost(id).subscribe({
      next: (blogPost) => {
        this.setBlogPost(blogPost);
      }, error: (error) => {
        console.error('Could not fetch blog post', error);
      }
    });
  }

  private setBlogPost(blogPost: BlogPost) {
    this.blogPost = blogPost;
    if (blogPost !== null) {
      this.titleService.setTitle(`Edit blog post: ${blogPost.title}`);
      this.oldBlogPostTitle = blogPost.title;
    }
    this.blogPostDiffer = this.differs.find(this.blogPost).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnInit() {
  }

  oldBlogPostTitle: string = '';
  blogPost: BlogPost = {
    id: 0,
    title: '',
    headline: '',
    body: '',
    author: null,
    published: new Date(0)
  };

  updateBlogPost() {
    this.blogPostService.updateBlogPost(this.blogPost).subscribe({
      next: (blogPost) => {
        this.hasChanges = false;
        this.router.navigate(['/community', 'blog', blogPost.id, this.slugifyPipe.transform(blogPost.title)]);
      }, error: (error) => {
        console.error(error);
      }
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private blogPostDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.blogPostDiffer !== null) {
      const changes = this.blogPostDiffer.diff(this.blogPost);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

}
