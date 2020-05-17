export class PaginationRequest {
  perPage: number;
  page: number;
  sortProperty: string;
  sortDirection: 'ascending' | 'descending';
}
