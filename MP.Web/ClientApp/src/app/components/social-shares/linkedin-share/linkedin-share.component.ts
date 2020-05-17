import { Component, OnInit, AfterViewChecked, IterableDiffers, Inject, ViewChild, IterableDiffer, ElementRef, Input } from '@angular/core';
import { Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';

@Component({
  selector: 'app-linkedin-share',
  templateUrl: './linkedin-share.component.html',
  styleUrls: ['./linkedin-share.component.scss'],
  host: {
    '[class.d-inline-block]': 'true',
    '[class.align-top]': 'true'
  }
})
export class LinkedinShareComponent implements AfterViewChecked {

  constructor(private router: Router, private locationStrategy: LocationStrategy, differs: IterableDiffers, @Inject('BASE_URL') private baseUrl: string) {
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
    let urlTree = this.router.createUrlTree(this.commands);
    let urlSerialized = this.router.serializeUrl(urlTree);
    this.href = this.baseUrl + this.locationStrategy.prepareExternalUrl(urlSerialized);
  }

  private reloadTwitterWidgets() {
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        //this.wrapper.nativeElement.innerHTML = `<a href="https://twitter.com/share" class="twitter-share-button" data-url="${this.href}" data-size="${this.size}" data-text="${this.text}" data-count="none">Tweet</a>`;
        this.wrapper.nativeElement.innerHTML = `<script type="IN/Share" data-url="${this.href}" data-size="${this.size}" data-text="${this.text}"></script>`;
        window['IN'] && window['IN'].parse();
      }, 10);
    }
  }

  @Input() text: string = '';
  @Input() size: 'large' | 'small' = 'large';

}
