import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TranslateModule } from '@ngx-translate/core';
import { QUERY_PARAMS_CONFIG, QueryParamsConfig, AdvancedRouterModule } from '@mintplayer/ng-router';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ControlsModule } from './controls/controls.module';
import { ComponentsModule } from './components/components.module';
import { environment } from '../environments/environment';
import { LinifyPipe } from './pipes/linify/linify.pipe';

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
    TranslateModule.forChild(),
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production }),
    AppRoutingModule,
    AdvancedRouterModule
  ],
  providers: [
    LinifyPipe,
    {
      provide: QUERY_PARAMS_CONFIG, useValue: <QueryParamsConfig>{
        'lang': 'preserve',
        'return': ''
      }
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
