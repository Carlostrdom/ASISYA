import 'zone.js';
import { NgZone } from '@angular/core';
import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';

platformBrowser().bootstrapModule(AppModule, {
  ngZone: new NgZone({ enableLongStackTrace: false })
}).catch((err: unknown) => console.error(err));
