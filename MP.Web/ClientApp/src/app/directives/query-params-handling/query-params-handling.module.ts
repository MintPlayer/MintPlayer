import { NgModule } from "@angular/core";
import { CommonModule } from '@angular/common';
import { QueryParamsHandlingDirective } from './query-params-handling/query-params-handling.directive';

@NgModule({
  declarations: [
    QueryParamsHandlingDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    QueryParamsHandlingDirective
  ]
})
export class QueryParamsHandlingModule {
}
