import { Component, OnInit, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BlogPostService } from '../../../../services/blog-post/blog-post.service';
import { Router, ActivatedRoute } from '@angular/router';
import { SlugifyPipe } from '../../../../pipes/slugify/slugify.pipe';
import { BlogPost } from '../../../../entities/blog-post';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private blogPostService: BlogPostService, private router: Router, private route: ActivatedRoute, private titleService: Title, private slugifyPipe: SlugifyPipe) {
    if (serverSide === false) {
      // Get blogpost
      let id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.blogPostService.getBlogPost(id).then(blogPost => {
        this.blogPost = blogPost;
        this.titleService.setTitle(`Edit blog post: ${blogPost.title}`);
        this.oldBlogPostTitle = blogPost.title;
      }).catch((error) => {
        console.log('Could not fetch blog post', error);
      });
    }
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
      this.router.navigate(['/community', 'blog', blogPost.id, this.slugifyPipe.transform(blogPost.title)]);
    }).catch((error) => {
      console.log(error);
    });
  }

}
