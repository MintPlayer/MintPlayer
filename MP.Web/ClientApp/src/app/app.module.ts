import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TranslateModule } from '@ngx-translate/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ControlsModule } from './controls/controls.module';
import { ComponentsModule } from './components/components.module';
import { PipesModule } from './pipes/pipes.module';
import { environment } from '../environments/environment';
import { LinifyPipe } from './pipes/linify/linify.pipe';
import { DirectivesModule } from './directives/directives.module';
import { QueryParamsHandlingModule } from './directives/query-params-handling/query-params-handling.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    BrowserAnimationsModule,
    HttpClientModule,
    ControlsModule,
    ComponentsModule,
    QueryParamsHandlingModule, // For language queryparam
    TranslateModule.forChild(),
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production }),
    AppRoutingModule,
  ],
  providers: [
    LinifyPipe
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
