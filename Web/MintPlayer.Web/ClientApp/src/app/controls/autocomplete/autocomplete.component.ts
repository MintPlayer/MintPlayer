import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from '../../interfaces/subject';
import { IAutocompleteElement } from './IAutocompleteElement';

@Component({
  selector: 'autocomplete',
  templateUrl: './autocomplete.component.html',
  styleUrls: ['./autocomplete.component.scss']
})
export class AutocompleteComponent implements OnInit {

  constructor(private httpClient: HttpClient) {
  }

  @Input() searchterm: string;
  @Input() suggestionUrl: string;
  @Input() suggestionMethod: string;

  dropdownVisible: boolean = false;
  clickedOutside($event: Event) {
    this.dropdownVisible = false;
  }

  provideSuggestions(value: string) {
    this.dropdownVisible = true;

    this.searchterm = value;
    this.searchtermChange.emit(this.searchterm);

    if (this.searchterm === '') {
      this.dropdownVisible = false;
      this.suggestions = [];
    } else {
      switch (this.suggestionMethod.toLowerCase()) {
        case 'get':
          this.httpClient.get<Subject[]>(`${this.suggestionUrl}/${this.searchterm}`).subscribe((items) => {
            this.suggestions = items;
          });
          break;
        case 'post':
          break;
        default:
          throw 'Invalid suggestion method';
      }
    }
  }

  suggestions: IAutocompleteElement[] = [];
  suggestionClicked(suggestion: IAutocompleteElement) {
    this.searchterm = suggestion.text;
    this.searchtermChange.emit(this.searchterm);

    this.dropdownVisible = false;
    this.suggestionSelected.emit(suggestion);
  }

  // Search by suggestion
  @Output() suggestionSelected: EventEmitter<IAutocompleteElement> = new EventEmitter();
  @Output() searchtermChange: EventEmitter<string> = new EventEmitter();

  // Search just by text
  @Output() submit: EventEmitter<string> = new EventEmitter();
  onSubmit() {
    this.dropdownVisible = false;
    this.submit.emit(this.searchterm);
  }

  ngOnInit() {
  }

}
