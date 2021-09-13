import { NgModule } from '@angular/core';
import { HttpClientModule, HttpClientXsrfModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { TranslateModule } from '@ngx-translate/core';
import { QUERY_PARAMS_CONFIG, QueryParamsConfig, AdvancedRouterModule } from '@mintplayer/ng-router';
import { BaseUrlOptions, BASE_URL, BASE_URL_OPTIONS } from '@mintplayer/ng-base-url';
import { API_VERSION } from '@mintplayer/ng-client';
import { VideoPlayerModule } from '@mintplayer/ng-video-player';
import { PlaylistControllerModule } from '@mintplayer/ng-playlist-controller';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { LinifyPipe } from './pipes/linify/linify.pipe';
import { EXTERNAL_URL } from './providers/external-url.provider';
import { SidebarModule } from './components/main-sidebar/main-sidebar.module';
import { PlaylistSidebarModule } from './components/playlist-sidebar/playlist-sidebar.module';
import { NavbarTogglerModule } from './controls/navbar-toggler/navbar-toggler.module';
import { PlaylistTogglerModule } from './controls/playlist-toggler/playlist-toggler.module';
import { CardModule } from './controls/card/card.module';
import { ModalModule } from './components/modal/modal.module';
import { FormsModule } from '@angular/forms';

const getExternalUrl = (baseUrl: string) => {
  if (new RegExp("\\blocalhost\\b").test(baseUrl)) {
    return baseUrl;
  } else {
    let match = new RegExp("^(http[s]{0,1})\\:\\/\\/(.*)$").exec(baseUrl);

    if (match === null) {

      let noSchemeMatch = new RegExp("^\\/\\/(.*)$").exec(baseUrl);
      let url = noSchemeMatch[1];
      return `//external.${url}`;
    } else {
      let protocol = match[1];
      let url = match[2];

      return `${protocol}://external.${url}`;
    }
  }

  //return baseUrl;
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    FormsModule,
    BrowserAnimationsModule,
    HttpClientModule,
    HttpClientXsrfModule.withOptions({
      cookieName: 'XSRF-TOKEN',
      headerName: 'X-XSRF-TOKEN'
    }),
    TranslateModule.forChild(),
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production }),
    AdvancedRouterModule,
    PlaylistControllerModule,

    CardModule,
    ModalModule,
    SidebarModule,
    VideoPlayerModule,
    NavbarTogglerModule,
    PlaylistTogglerModule,
    PlaylistSidebarModule,

    AppRoutingModule,
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
    }, {
      // Drop the scheme from the base-url provider
      provide: BASE_URL_OPTIONS,
      useValue: <BaseUrlOptions>{
        dropScheme: true
      }
    }, {
      provide: API_VERSION,
      useValue: 'v3'
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
