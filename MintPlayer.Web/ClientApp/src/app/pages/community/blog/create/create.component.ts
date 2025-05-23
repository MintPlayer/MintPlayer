import { Component, OnInit, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { BlogPost, BlogPostService } from '@mintplayer/ng-client';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    private titleService: Title,
    private blogPostService: BlogPostService,
    private router: AdvancedRouter,
    private slugifyPipe: SlugifyPipe,
    private differs: KeyValueDiffers,
  ) {
    this.titleService.setTitle('Write new blog post');
  }

  blogPost: BlogPost = {
    id: 0,
    title: '',
    headline: '',
    body: '',
    author: null,
    published: new Date(0)
  };

  saveBlogPost() {
    this.blogPostService.createBlogPost(this.blogPost).subscribe({
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

  ngOnInit() {
    this.blogPostDiffer = this.differs.find(this.blogPost).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
  }

}
