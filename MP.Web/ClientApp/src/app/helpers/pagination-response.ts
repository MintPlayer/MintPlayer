export class PaginationResponse<T> {
  data: T[];

  page: number;
  perPage: number;
  totalRecords: number;
  totalPages: number;
}
