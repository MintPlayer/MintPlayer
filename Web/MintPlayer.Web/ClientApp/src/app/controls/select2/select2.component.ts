import { Component, OnInit, ElementRef, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ISelect2Element } from './ISelect2Element';

@Component({
  selector: 'app-select2',
  templateUrl: './select2.component.html',
  styleUrls: ['./select2.component.scss'],
	host: {
    "(click)": "focusTextbox()"
  }
})
export class Select2Component implements OnInit {
  constructor(private httpClient: HttpClient) {
  }

  searchText: string = "";
  dropdownVisible: boolean = false;

  @ViewChild('searchbox', { static: false }) searchbox: ElementRef;

  //#region Items
  @Input() items: ISelect2Element[];
  @Output() itemChanged: EventEmitter<[ISelect2Element, string]> = new EventEmitter();
  itemClicked($event: MouseEvent) {
    $event.stopPropagation();
  }
  removeItem(item: ISelect2Element, $event: MouseEvent) {
    $event.stopPropagation();
    this.itemChanged.emit([item, 'remove']);
  }
  //#endregion
  //#region Suggestions
  suggestions: ISelect2Element[] = [];
  private provideSuggestions() {
    switch (this.method) {
      case 'get':
        this.httpClient.get(this.url + "/" + this.searchText, {
          headers: this.headers
        }).subscribe((response: ISelect2Element[]) => {
          this.suggestions = response;
        });
        break;
      case 'post':
        this.httpClient.post(this.url, {
          searchTerm: this.searchText
        }, {
          headers: this.headers
        }).subscribe((response: ISelect2Element[]) => {
          this.suggestions = response;
        });
        break;
      default:
        throw "invalid method";
    }
  }
  suggestionClicked(suggestion: ISelect2Element) {
    this.searchText = '';
    this.dropdownVisible = false;

    var existing = this.items.find((value, index) => value.id === suggestion.id);
    if (existing === undefined) {
      this.itemChanged.emit([suggestion, 'add']);
    } else {
      this.itemChanged.emit([existing, 'remove']);
    }
  }
  //#endregion
  //#region properties
  @Input() url: string;
  @Input() method: string;
  @Input() headers: HttpHeaders;
  //#endregion

  textChanged() {
    this.updateTextWidth();
    this.provideSuggestions();
  }

  //#region Textbox width
  charWidth: number = 10;
  searchWidth: number = 20;
  private updateTextWidth() {
    this.searchWidth = this.charWidth * (this.searchText.length + 2);
    this.dropdownVisible = true;
  }
  //#endregion
  //#region Click outside
  clickedOutside($event: Event) {
    this.dropdownVisible = false;
  }
  focusTextbox() {
    this.searchbox.nativeElement.focus();
  }
  //#endregion

  ngOnInit() {
  }
}
