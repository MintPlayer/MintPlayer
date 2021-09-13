import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { BASE_URL } from '@mintplayer/ng-base-url';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss']
})
export class NotFoundComponent implements OnInit, OnDestroy {

  constructor(
    private metaService: Meta,
    @Inject(BASE_URL) private baseUrl: string,
  ) {
  }

  ngOnInit() {
    this.addMetaTags();
  }

  ngOnDestroy() {
    this.removeMetaTags();
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      itemprop: 'name',
      content: 'MintPlayer'
    }, {
      name: 'description',
      itemprop: 'description',
      content: 'MintPlayer is an open-source project that lets you keep track of the music you like. Start building your playlist now.'
    }, {
      itemprop: 'image',
      content: `${this.baseUrl}/assets/logo/music_note_72.png`
    }, {
      itemprop: 'publisher',
      name: 'creator',
      content: 'MintPlayer LLC'
    }, {
      itemprop: 'copyrightYear',
      content: new Date().getFullYear().toString()
    }, {
      itemprop: 'copyrightHolder',
      content: 'MintPlayer LLC'
    }, {
      itemprop: 'isFamilyFriendly',
      content: 'true'
    }]);
  }
  private removeMetaTags() {
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion
}
