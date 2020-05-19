import { Component, OnInit, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BlogPost } from '../../../../entities/blog-post';
import { BlogPostService } from '../../../../services/blog-post/blog-post.service';
import { Router } from '@angular/router';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  constructor(private titleService: Title, private blogPostService: BlogPostService, private router: Router, private slugifyPipe: SlugifyPipe) {
    this.titleService.setTitle('Write new blog post');
  }

  ngOnInit() {
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

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }

}
