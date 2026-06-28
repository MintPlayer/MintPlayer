import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

import { RecoveryRoutingModule } from './recovery-routing.module';
import { RecoveryComponent } from './recovery.component';
import { FocusOnLoadDirectiveModule } from '../../../../directives/focus-on-load/focus-on-load.module';


@NgModule({
  declarations: [
    RecoveryComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    FocusOnLoadDirectiveModule,
    RecoveryRoutingModule
  ]
})
export class RecoveryModule { }
