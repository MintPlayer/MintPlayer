import { Component, OnInit, Input, AfterViewInit, IterableDiffers, IterableDiffer, AfterViewChecked } from '@angular/core';
import { Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';

@Component({
  selector: 'app-facebook-share',
  templateUrl: './facebook-share.component.html',
  styleUrls: ['./facebook-share.component.scss'],
  host: {
    '[class.d-inline-block]': 'true',
    '[class.align-middle]': 'true'
  }
})
export class FacebookShareComponent implements OnInit, AfterViewInit, AfterViewChecked {

  constructor(private router: Router, private locationStrategy: LocationStrategy, differs: IterableDiffers) {
    this.differ = differs.find(this.commands).create(null);
  }

  differ: IterableDiffer<any>;

  ngOnInit() {
  }

  ngAfterViewInit() {
    //console.log('ngAfterViewInit');
    this.updateTargetUrlAndHref();
  }

  ngAfterViewChecked() {
    //console.log('ngAfterViewChecked');
    const change = this.differ.diff(this.commands);
    if (change !== null) {
      console.log(change);
      //this.updateTargetUrlAndHref();
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
    this.updateTargetUrlAndHref();
  }
  //#endregion

  private updateTargetUrlAndHref() {
    let urlTree = this.router.createUrlTree(this.commands);
    let urlSerialized = this.router.serializeUrl(urlTree);
    this.href = this.locationStrategy.prepareExternalUrl(urlSerialized);
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        window['FB'] && window['FB'].XFBML.parse();
      }, 20);
    }
  }

  @Input() size: 'large' | 'small' = 'large';
  @Input() layout: 'box_count' | 'button_count' | 'button' = 'button_count';

}
