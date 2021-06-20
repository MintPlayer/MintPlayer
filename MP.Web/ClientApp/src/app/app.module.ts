import { NgModule, Optional } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TranslateModule } from '@ngx-translate/core';
//import { BaseUrlModule, BASE_URL } from '@mintplayer/ng-base-url';
import { QUERY_PARAMS_CONFIG, QueryParamsConfig, AdvancedRouterModule } from '@mintplayer/ng-router';
import { YoutubePlayerModule } from '@mintplayer/ng-youtube-player';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ControlsModule } from './controls/controls.module';
import { ComponentsModule } from './components/components.module';
import { environment } from '../environments/environment';
import { LinifyPipe } from './pipes/linify/linify.pipe';
import { SERVER_SIDE_BIS, TEST } from './test.provider';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    BrowserAnimationsModule,
    HttpClientModule,
    YoutubePlayerModule,
    ControlsModule,
    ComponentsModule,
    TranslateModule.forChild(),
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production }),
    AppRoutingModule,
    AdvancedRouterModule,
    //BaseUrlModule,
  ],
  providers: [
    LinifyPipe,
    {
      provide: QUERY_PARAMS_CONFIG, useValue: <QueryParamsConfig>{
        'lang': 'preserve',
        'return': ''
      }
    },
    {
      provide: TEST,
      useFactory: getTest, deps: [ [new Optional(), SERVER_SIDE_BIS] ]
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function getTest(serverSide?: boolean) {
  if (serverSide === null) {
    return 'null';
  } else if (serverSide === true) {
    return 'true';
  } else {
    return 'false';
  }
}
