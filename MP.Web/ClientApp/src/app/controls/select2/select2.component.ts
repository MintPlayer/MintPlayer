import { Component, OnInit, ViewChild, ElementRef, Input, Output, EventEmitter, Inject, TemplateRef, HostBinding } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AutocompleteElement } from '../autocomplete-element';

@Component({
  selector: 'select2',
  templateUrl: './select2.component.html',
  styleUrls: ['./select2.component.scss'],
  host: {
    '(click)': 'focusTextbox()',
    '[class.form-control-normal]': 'true'
  }
})
export class Select2Component implements OnInit {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  searchText: string = '';
  dropdownVisible: boolean = false;

  @ViewChild('searchbox') searchbox: ElementRef;

  //#region Items
  @Input() items: AutocompleteElement[];
  @Output() itemChanged: EventEmitter<[AutocompleteElement, 'add' | 'remove']> = new EventEmitter();
  itemClicked($event: MouseEvent) {
    $event.stopPropagation();
  }
  removeItem(item: AutocompleteElement, $event: MouseEvent) {
    $event.stopPropagation();
    this.items.splice(this.items.indexOf(item), 1);
    this.itemChanged.emit([item, 'remove']);
  }
  //#endregion
  //#region Suggestions
  suggestions: AutocompleteElement[] = [];
  private provideSuggestions() {
    switch (this.method) {
      case 'get':
        this.httpClient.get(`${this.baseUrl}${this.url}/${this.searchText}`, {
          headers: this.headers
        }).subscribe((response: AutocompleteElement[]) => {
          this.suggestions = response;
        });
        break;
      case 'post':
        this.httpClient.post(this.baseUrl + this.url, {
          searchTerm: this.searchText
        }, {
          headers: this.headers
        }).subscribe((response: AutocompleteElement[]) => {
          this.suggestions = response;
        });
        break;
      default:
        throw 'invalid method';
    }
  }
  suggestionClicked(suggestion: AutocompleteElement) {
    this.searchText = '';
    this.dropdownVisible = false;

    var existing = this.items.find((value, index) => value.id === suggestion.id);
    if (existing === undefined) {
      this.items.push(suggestion);
      this.itemChanged.emit([suggestion, 'add']);
    } else {
      this.items.splice(this.items.indexOf(existing), 1);
      this.itemChanged.emit([existing, 'remove']);
    }
  }
  //#endregion
  //#region properties
  @Input() url: string;
  @Input() method: string;
  @Input() headers: HttpHeaders;
  //#endregion

  @ViewChild('defaultItemTemplate', { static: true }) defaultItemTemplate: TemplateRef<any>;
  @Input() itemTemplate: TemplateRef<any>;
  get getItemTemplate() {
    if (this.itemTemplate) return this.itemTemplate;
    else return this.defaultItemTemplate;
  }

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

  @HostBinding('class.form-control-focus') isFocused: boolean = false;
  public focus() {
    this.focusTextbox();
  }

  ngOnInit() {
  }
}
