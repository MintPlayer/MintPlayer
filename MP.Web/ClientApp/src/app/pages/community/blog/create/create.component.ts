import { Component, OnInit, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BlogPost } from '../../../../entities/blog-post';
import { BlogPostService } from '../../../../services/blog-post/blog-post.service';
import { Router } from '@angular/router';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';
import { ExtendedRouter } from '../../../../helpers/extended-router';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(private titleService: Title, private blogPostService: BlogPostService, private router: ExtendedRouter, private slugifyPipe: SlugifyPipe, private differs: KeyValueDiffers) {
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
    this.blogPostService.createBlogPost(this.blogPost).then((blogPost) => {
      this.router.navigate(['/community', 'blog', blogPost.id, this.slugifyPipe.transform(blogPost.title)]);
    }).catch((error) => {
      console.log(error);
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
