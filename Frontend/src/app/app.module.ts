import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { HttpClient } from '@angular/common/http';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { SharedPipesModule } from './shared/pipes/shared-pipes.module';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { SessionTimeoutHandlerComponent } from './shared/components/session-timeout-handler/session-timeout-handler.component';
import { SignalrToastComponent } from './shared/components/signalr-toast/signalr-toast.component';
import { ImpersonationModule } from './shared/impersonation.module';

// Factory function to load translation files
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, 'assets/i18n/', '.json');
}

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent,
    AuthLayoutComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    MatSnackBarModule,
    AppRoutingModule,
    SharedPipesModule,
    SessionTimeoutHandlerComponent,
    SignalrToastComponent,
    ImpersonationModule,
    TranslateModule.forRoot({
      defaultLanguage: localStorage.getItem('lang') == 'en' ? 'en' : 'ar',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
