import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ClickOutsideModule } from 'ng-click-outside';
import { NavbarTogglerComponent } from './navbar-toggler/navbar-toggler.component';
import { CardComponent } from './card/card.component';
import { Select2Component } from './select2/select2.component';
import { PipesModule } from '../pipes/pipes.module';
import { AutocompleteComponent } from './autocomplete/autocomplete.component';
import { PlaylistTogglerComponent } from './playlist-toggler/playlist-toggler.component';
import { ProgressBarComponent } from './progress-bar/progress-bar.component';
import { PaginationComponent } from './pagination/pagination.component';
import { DatatableComponent } from './datatable/datatable.component';
import { SortableListComponent } from './sortable-list/sortable-list.component';
import { DropdownSearchBoxComponent } from './dropdown-search-box/dropdown-search-box.component';
import { PopupComponent } from './popup/popup.component';
import { ToggleButtonComponent } from './toggle-button/toggle-button.component';



@NgModule({
  declarations: [
    NavbarTogglerComponent,
    CardComponent,
    Select2Component,
    AutocompleteComponent,
    PlaylistTogglerComponent,
    ProgressBarComponent,
    PaginationComponent,
    DatatableComponent,
    SortableListComponent,
    DropdownSearchBoxComponent,
    PopupComponent,
    ToggleButtonComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ClickOutsideModule,
    PipesModule
  ],
  exports: [
    NavbarTogglerComponent,
    CardComponent,
    Select2Component,
    AutocompleteComponent,
    PlaylistTogglerComponent,
    ProgressBarComponent,
    PaginationComponent,
    DatatableComponent,
    SortableListComponent,
    DropdownSearchBoxComponent,
    PopupComponent,
    ToggleButtonComponent
  ]
})
export class ControlsModule { }
