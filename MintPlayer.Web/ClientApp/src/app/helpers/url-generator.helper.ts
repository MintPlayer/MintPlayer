import { Inject, Injectable } from '@angular/core';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { Artist, BlogPost, Person, Song } from '@mintplayer/ng-client';
import { SlugifyHelper } from './slugify.helper';

@Injectable({
  providedIn: 'root'
})
export class UrlGenerator {
  constructor(
    @Inject(BASE_URL) private baseUrl: string,
    private slugifyHelper: SlugifyHelper,
  ) {
  }

  public generateFullUrl(subject: Person | Artist | Song | BlogPost) {
    if ('firstName' in subject) {
      // Person
      return `${this.baseUrl}/person/${subject.id}/${this.slugifyHelper.slugify(subject.text)}`;
    } else if ('name' in subject) {
      // Artist
      return `${this.baseUrl}/artist/${subject.id}/${this.slugifyHelper.slugify(subject.text)}`;
    } else if ('released' in subject) {
      // Song
      return `${this.baseUrl}/song/${subject.id}/${this.slugifyHelper.slugify(subject.text)}`;
    } else if ('headline' in subject) {
      // BlogPost
      return `${this.baseUrl}/community/blog/${subject.id}/${this.slugifyHelper.slugify(subject.headline)}`;
    } else {
      throw 'Subject type not recognized';
    }
  }

  public generateCommands(subject: Person | Artist | Song | BlogPost) {
    if ('firstName' in subject) {
      // Person
      return ['/person', subject.id, this.slugifyHelper.slugify(subject.text)]
    } else if ('name' in subject) {
      // Artist
      return ['/artist', subject.id, this.slugifyHelper.slugify(subject.text)]
    } else if ('released' in subject) {
      // Song
      return ['/song', subject.id, this.slugifyHelper.slugify(subject.text)]
    } else if ('headline' in subject) {
      // BlogPost
      return ['/community', 'blog', subject.id, this.slugifyHelper.slugify(subject.headline)];
    } else {
      throw 'Subject type not recognized';
    }
  }
}
