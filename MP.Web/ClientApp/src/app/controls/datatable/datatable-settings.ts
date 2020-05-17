import { DataColumn } from './datacolumn';
import { ValueList } from './Value-list';
import { PaginationRequest } from '../../helpers/pagination-request';

export class DatatableSettings {
  constructor(data?: Partial<DatatableSettings>) {
    Object.assign(this, data);
  }

  columns: DataColumn[];
  perPages: ValueList;
  pages: ValueList;
  sortProperty: string;
  sortDirection: 'ascending' | 'descending';

  public toPaginationRequest() {
    return <PaginationRequest>{
      perPage: this.perPages.selected,
      page: this.pages.selected,
      sortProperty: this.sortProperty,
      sortDirection: this.sortDirection
    };
  }
}
