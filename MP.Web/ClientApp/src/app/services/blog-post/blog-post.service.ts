import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
//import { BASE_URL } from '@mintplayer/ng-base-url';
import { BlogPost } from '../../entities/blog-post';

@Injectable({
  providedIn: 'root'
})
export class BlogPostService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string, @Inject('API_VERSION') private apiVersion: string) {
  }

  public getBlogPosts() {
    return this.httpClient.get<BlogPost[]>(`${this.baseUrl}/web/${this.apiVersion}/blogpost`).toPromise();
  }

  public getBlogPost(id: number) {
    return this.httpClient.get<BlogPost>(`${this.baseUrl}/web/${this.apiVersion}/blogpost/${id}`).toPromise();
  }

  public createBlogPost(blogPost: BlogPost) {
    return this.httpClient.post<BlogPost>(`${this.baseUrl}/web/${this.apiVersion}/blogpost`, blogPost).toPromise();
  }

  public updateBlogPost(blogPost: BlogPost) {
    return this.httpClient.put<BlogPost>(`${this.baseUrl}/web/${this.apiVersion}/blogpost/${blogPost.id}`, blogPost).toPromise();
  }

  public deleteBlogPost(blogPost: BlogPost) {
    return this.httpClient.delete(`${this.baseUrl}/web/${this.apiVersion}/blogpost/${blogPost.id}`).toPromise();
  }

}
