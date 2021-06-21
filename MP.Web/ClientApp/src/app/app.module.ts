import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TranslateModule } from '@ngx-translate/core';
import { QUERY_PARAMS_CONFIG, QueryParamsConfig, AdvancedRouterModule } from '@mintplayer/ng-router';
import { YoutubePlayerModule } from '@mintplayer/ng-youtube-player';
import { BASE_URL } from '@mintplayer/ng-base-url';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ControlsModule } from './controls/controls.module';
import { ComponentsModule } from './components/components.module';
import { environment } from '../environments/environment';
import { LinifyPipe } from './pipes/linify/linify.pipe';
import { EXTERNAL_URL } from './providers/external-url.provider';

const getExternalUrl = (baseUrl: string) => {
  if (new RegExp("\\blocalhost\\b").test(baseUrl)) {
    return baseUrl;
  } else {
    let match = new RegExp("^(http[s]{0,1})\\:\\/\\/(.*)$").exec(baseUrl);

    let protocol = match[1];
    let url = match[2];

    return `${protocol}://external.${url}`;
  }
}

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
  ],
  providers: [
    LinifyPipe,
    {
      provide: QUERY_PARAMS_CONFIG, useValue: <QueryParamsConfig>{
        'lang': 'preserve',
        'return': ''
      }
    }, {
      provide: EXTERNAL_URL,
      useFactory: getExternalUrl,
      deps: [BASE_URL]
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
