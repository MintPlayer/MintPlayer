import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BlogPost } from '../../entities/blog-post';

@Injectable({
  providedIn: 'root'
})
export class BlogPostService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  public getBlogPosts() {
    return this.httpClient.get<BlogPost[]>(`${this.baseUrl}/web/v2/blogpost`).toPromise();
  }

  public getBlogPost(id: number) {
    return this.httpClient.get<BlogPost>(`${this.baseUrl}/web/v2/blogpost/${id}`).toPromise();
  }

  public createBlogPost(blogPost: BlogPost) {
    return this.httpClient.post<BlogPost>(`${this.baseUrl}/web/v2/blogpost`, blogPost).toPromise();
  }

  public updateBlogPost(blogPost: BlogPost) {
    return this.httpClient.put<BlogPost>(`${this.baseUrl}/web/v2/blogpost/${blogPost.id}`, blogPost).toPromise();
  }

  public deleteBlogPost(blogPost: BlogPost) {
    return this.httpClient.delete(`${this.baseUrl}/web/v2/blogpost/${blogPost.id}`).toPromise();
  }

}
