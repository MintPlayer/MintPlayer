import { Component, OnInit, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BlogPostService } from '../../../../services/blog-post/blog-post.service';
import { Router, ActivatedRoute } from '@angular/router';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';
import { BlogPost } from '../../../../entities/blog-post';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';
import { NavigationHelper } from '../../../../helpers/navigation.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, DoCheck, HasChanges {

  constructor(
    @Inject('SERVERSIDE') private serverSide: boolean,
    private blogPostService: BlogPostService,
    private navigation: NavigationHelper,
    private route: ActivatedRoute,
    private titleService: Title,
    private slugifyPipe: SlugifyPipe,
    private differs: KeyValueDiffers
  ) {
    if (serverSide === false) {
      // Get blogpost
      let id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadBlogPost(id);
    }
  }

  private loadBlogPost(id: number) {
    this.blogPostService.getBlogPost(id).then((blogPost) => {
      this.setBlogPost(blogPost);
    }).catch((error) => {
      console.log('Could not fetch blog post', error);
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
    this.blogPostService.updateBlogPost(this.blogPost).then((blogPost) => {
      this.hasChanges = false;
      this.navigation.navigate(['/community', 'blog', blogPost.id, this.slugifyPipe.transform(blogPost.title)]);
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

}
