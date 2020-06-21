import { Component, Input, Inject, IterableDiffers, IterableDiffer, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';
import { NavigationHelper } from '../../../helpers/navigation.helper';

@Component({
  selector: 'app-twitter-share',
  templateUrl: './twitter-share.component.html',
  styleUrls: ['./twitter-share.component.scss'],
  host: {
    '[class.d-inline-block]': 'true',
    '[class.align-middle]': 'true'
  }
})
export class TwitterShareComponent implements AfterViewChecked {

  constructor(
    private navigation: NavigationHelper,
    private locationStrategy: LocationStrategy,
    differs: IterableDiffers,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.differ = differs.find(this.commands).create(null);
  }

  @ViewChild('wrapper') wrapper: ElementRef<HTMLDivElement>;
  differ: IterableDiffer<any>;

  ngAfterViewChecked() {
    const change = this.differ.diff(this.commands);
    if (change !== null) {
      this.updateHref();
      this.reloadTwitterWidgets();
    }
  }

  //#region url
  href: string;
  private commands: any[] = [];
  @Input() set routerLink(value: string | any[]) {
    if (value === null) {
      this.commands = [];
    } else if (Array.isArray(value)) {
      this.commands = value;
    } else {
      this.commands = [value];
    }
    this.updateHref();
    this.reloadTwitterWidgets();
  }
  //#endregion


  private updateHref() {
    let urlTree = this.navigation.createUrlTree(this.commands);
    let urlSerialized = this.navigation.serializeUrl(urlTree);
    this.href = this.baseUrl + this.locationStrategy.prepareExternalUrl(urlSerialized);
  }

  private reloadTwitterWidgets() {
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        this.wrapper.nativeElement.innerHTML = `<a href="https://twitter.com/share" class="twitter-share-button" data-url="${this.href}" data-size="${this.size}" data-text="${this.text}" data-count="none">Tweet</a>`;
        window['twttr'] && window['twttr'].widgets.load();
      }, 10);
    }
  }

  @Input() text: string = '';
  @Input() size: 'large' | 'small' = 'large';
}
