import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private currentLangSubject = new BehaviorSubject<string>('en');
  public currentLang$: Observable<string> = this.currentLangSubject.asObservable();

  private readonly SUPPORTED_LANGUAGES = ['en', 'ar'];
  private readonly DEFAULT_LANGUAGE = 'en';
  private readonly STORAGE_KEY = 'lang';

  constructor(private translateService: TranslateService) {
    this.initializeLanguage();
  }

  /**
   * Initialize language service with supported languages
   */
  private initializeLanguage(): void {
    // Add supported languages to TranslateService
    this.translateService.addLangs(this.SUPPORTED_LANGUAGES);

    // Set default language
    this.translateService.setDefaultLang(this.DEFAULT_LANGUAGE);

    // Load saved language preference or use browser language
    const savedLang = this.getStoredLanguage();
    const browserLang = this.translateService.getBrowserLang() || null;
    const initialLang: string = this.isValidLanguage(savedLang) ? savedLang ?? this.DEFAULT_LANGUAGE :
                       this.isValidLanguage(browserLang) ? browserLang ?? this.DEFAULT_LANGUAGE :
                       this.DEFAULT_LANGUAGE;

    this.setLanguage(initialLang);
  }

  /**
   * Set current language
   */
  setLanguage(lang: string): void {
    if (!this.isValidLanguage(lang)) {
      console.warn(`Invalid language: ${lang}. Falling back to default.`);
      lang = this.DEFAULT_LANGUAGE;
    }

    this.translateService.use(lang).subscribe({
      next: () => {
        this.currentLangSubject.next(lang);
        this.storeLanguage(lang);
        this.applyLanguageDirection(lang);
        console.log('Language successfully changed to:', lang);
      },
      error: (error) => {
        console.error('Error changing language:', error);
        // Fallback: still update the current language
        this.currentLangSubject.next(lang);
        this.storeLanguage(lang);
        this.applyLanguageDirection(lang);
      }
    });
  }

  /**
   * Get current language
   */
  getCurrentLanguage(): string {
    return this.currentLangSubject.value;
  }

  /**
   * Toggle between supported languages
   */
  toggleLanguage(): void {
    const currentLang = this.getCurrentLanguage();
    const newLang = currentLang === 'ar' ? 'en' : 'ar';
    this.setLanguage(newLang);
  }

  /**
   * Get language display name
   */
  getLanguageDisplay(lang?: string): string {
    const language = lang || this.getCurrentLanguage();
    return language === 'ar' ? 'العربية' : 'English';
  }

  /**
   * Check if language is RTL
   */
  isRTL(lang?: string): boolean {
    const language = lang || this.getCurrentLanguage();
    return language === 'ar';
  }

  /**
   * Get all supported languages
   */
  getSupportedLanguages(): string[] {
    return [...this.SUPPORTED_LANGUAGES];
  }

  /**
   * Check if language is supported
   */
  private isValidLanguage(lang: string | null): boolean {
    return lang !== null && this.SUPPORTED_LANGUAGES.includes(lang);
  }

  /**
   * Store language preference in localStorage
   */
  private storeLanguage(lang: string): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, lang);
    } catch (e) {
      console.warn('Could not store language preference:', e);
    }
  }

  /**
   * Get stored language preference
   */
  private getStoredLanguage(): string | null {
    try {
      return localStorage.getItem(this.STORAGE_KEY);
    } catch (e) {
      console.warn('Could not retrieve language preference:', e);
      return null;
    }
  }

  /**
   * Apply language direction to document
   */
  private applyLanguageDirection(lang: string): void {
    const isRTL = this.isRTL(lang);
    const htmlElement = document.documentElement;

    htmlElement.setAttribute('dir', isRTL ? 'rtl' : 'ltr');
    htmlElement.setAttribute('lang', lang);

    document.body.classList.remove('rtl', 'ltr');
    document.body.classList.add(isRTL ? 'rtl' : 'ltr');
  }
}
