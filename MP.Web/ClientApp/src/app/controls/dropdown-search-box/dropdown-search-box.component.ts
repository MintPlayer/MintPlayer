import { Component, OnInit, Input, ViewChild, ElementRef, Inject, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AutocompleteElement } from '../autocomplete-element';

@Component({
  selector: 'dropdown-search-box',
  templateUrl: './dropdown-search-box.component.html',
  styleUrls: ['./dropdown-search-box.component.scss']
})
export class DropdownSearchBoxComponent implements OnInit {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  @Input() url: string;
  @Input() method: string;
  @Input() headers: HttpHeaders;
  @Input() searchPlaceholder: string;
  @Input() searchingCaption: string = 'Searching';
  @Input() noResultsCaption: string = 'No results';
  @Input() textPropertyPath: string = 'text';

  @Output() suggestionClicked: EventEmitter<any> = new EventEmitter();

  @ViewChild('searchBox') searchBox: ElementRef<HTMLInputElement>;

  isOpen: boolean = false;
  search: string = '';
  busy: boolean = false;
  suggestions: any[] = [];

  toggleDropdown() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      setTimeout(() => {
        console.log(this.searchBox);
        this.searchBox.nativeElement.focus();
      }, 10);
    }
    return false;
  }
  performSearch() {
    if (!!this.search) {
      this.busy = true;
      switch (this.method) {
        case 'get':
          this.httpClient.get<AutocompleteElement[]>(`${this.baseUrl}${this.url}/${this.search}`, {
            headers: this.headers
          }).subscribe((response) => {
            this.suggestions = response;
          }, (error) => {
            console.log(error);
          }, () => {
            this.busy = false;
          });
          break;
        case 'post':
          this.httpClient.post<AutocompleteElement[]>(`${this.baseUrl}${this.url}`, {
            searchTerm: this.search
          }, {
            headers: this.headers
          }).subscribe((response) => {
            this.suggestions = response;
          }, (error) => {
            console.log(error);
          }, () => {
            this.busy = false;
          });
          break;
        default:
          throw 'invalid method';
      }
    } else {
      this.suggestions = [];
    }
  }

  onSuggestionClicked(suggestion: any) {
    this.isOpen = false;
    this.search = '';
    this.suggestionClicked.emit(suggestion);
  }
  clickedOutside($event: Event) {
    this.isOpen = false;
  }

  ngOnInit() {
  }

}
