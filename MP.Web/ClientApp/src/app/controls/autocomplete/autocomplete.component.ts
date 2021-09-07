import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AutocompleteElement } from '../autocomplete-element';
import { Subject } from '@mintplayer/ng-client';

@Component({
  selector: 'autocomplete',
  templateUrl: './autocomplete.component.html',
  styleUrls: ['./autocomplete.component.scss']
})
export class AutocompleteComponent implements OnInit {

  constructor() {
  }

  @Input() searchterm: string;

  dropdownVisible: boolean = false;
  clickedOutside($event: Event) {
    this.dropdownVisible = false;
  }

  onProvideSuggestions(value: string) {
    this.searchterm = value;
    this.searchtermChange.emit(this.searchterm);

    if (this.searchterm === '') {
      this.dropdownVisible = false;
      this.suggestions = [];
    } else {
      this.dropdownVisible = true;
      this.provideSuggestions.emit(value);
    }
  }

  @Output() public provideSuggestions: EventEmitter<string> = new EventEmitter();
  @Input() public suggestions: AutocompleteElement[] = [];
  suggestionClicked(suggestion: AutocompleteElement) {
    this.searchterm = suggestion.text;
    this.searchtermChange.emit(this.searchterm);

    this.dropdownVisible = false;
    this.suggestionSelected.emit(suggestion);
  }

  // Search by suggestion
  @Output() suggestionSelected: EventEmitter<AutocompleteElement> = new EventEmitter();
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
