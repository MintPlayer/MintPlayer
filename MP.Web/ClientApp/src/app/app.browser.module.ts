import { NgModule } from "@angular/core";
import { AppComponent } from './app.component';
import { AppModule } from './app.module';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

@NgModule({
  imports: [
    AppModule,
    HttpClientModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: (http: HttpClient) => {
          return new TranslateHttpLoader(http);
        },
        deps: [
          HttpClient
        ]
      }
    })
  ],
  bootstrap: [AppComponent]
})
export class AppBrowserModule {
}
