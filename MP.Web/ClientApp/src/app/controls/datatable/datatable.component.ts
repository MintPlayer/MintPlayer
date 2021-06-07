import { Component, OnInit, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { DatatableSettings } from './datatable-settings';
import { PaginationResponse } from '../../helpers/pagination-response';
import { DataColumn } from './datacolumn';

@Component({
  selector: 'datatable',
  templateUrl: './datatable.component.html',
  styleUrls: ['./datatable.component.scss']
})
export class DatatableComponent implements OnInit {

  constructor() {
  }

  @Input() settings: DatatableSettings;
  @Input() data: PaginationResponse<any>;
  @Input() rowTemplate: TemplateRef<any>;
  @Output() onReloadData: EventEmitter<any> = new EventEmitter();

  columnHeaderClicked(column: DataColumn) {
    if (column.sortable) {
      if (this.settings.sortProperty !== column.name) {
        this.settings.sortProperty = column.name;
        this.settings.sortDirection = 'ascending';
      } else if (this.settings.sortDirection === 'descending') {
        this.settings.sortDirection = 'ascending';
      } else {
        this.settings.sortDirection = 'descending';
      }
      this.onReloadData.emit();
    }
  }

  ngOnInit() {
  }

}
