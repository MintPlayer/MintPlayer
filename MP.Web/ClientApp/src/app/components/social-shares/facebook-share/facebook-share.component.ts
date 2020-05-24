import { Component, OnInit, Input, AfterViewInit, IterableDiffers, IterableDiffer, AfterViewChecked, ViewChild, ElementRef, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';
import { ExtendedRouter } from '../../../helpers/extended-router';

@Component({
  selector: 'app-facebook-share',
  templateUrl: './facebook-share.component.html',
  styleUrls: ['./facebook-share.component.scss'],
  host: {
    '[class.d-inline-block]': 'true',
    '[class.align-middle]': 'true'
  }
})
export class FacebookShareComponent implements OnInit, AfterViewChecked {

  constructor(private router: ExtendedRouter, private locationStrategy: LocationStrategy, differs: IterableDiffers, @Inject('BASE_URL') private baseUrl: string) {
    this.differ = differs.find(this.commands).create(null);
  }

  @ViewChild('wrapper') wrapper: ElementRef<HTMLDivElement>;
  differ: IterableDiffer<any>;

  ngOnInit() {
  }

  ngAfterViewChecked() {
    //console.log('ngAfterViewChecked');
    const change = this.differ.diff(this.commands);
    if (change !== null) {
      this.updateHref();
      this.reloadFacebookWidgets();
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
    this.reloadFacebookWidgets();
  }
  //#endregion

  private updateHref() {
    let urlTree = this.router.createUrlTree(this.commands);
    let urlSerialized = this.router.serializeUrl(urlTree);
    this.href = this.baseUrl + this.locationStrategy.prepareExternalUrl(urlSerialized);
  }

  private reloadFacebookWidgets() {
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        this.wrapper.nativeElement.innerHTML = `<div class="fb-share-button" data-href="${this.href}" data-size="${this.size}" data-layout="${this.layout}"></div>`;
        window['FB'] && window['FB'].XFBML.parse();
      }, 20);
    }
  }

  @Input() size: 'large' | 'small' = 'large';
  @Input() layout: 'icon_link' | 'box_count' | 'button_count' | 'button' = 'button_count';

}
