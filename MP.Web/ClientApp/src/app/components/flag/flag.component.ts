import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-flag',
  templateUrl: './flag.component.html',
  styleUrls: ['./flag.component.scss'],
  host: {
    '[class.d-inline-block]': 'true',
  }
})
export class FlagComponent implements OnInit {

  constructor() {
  }

  ngOnInit() {
  }

  @Input() country: string;
}
