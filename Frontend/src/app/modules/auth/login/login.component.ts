import { Component, HostListener } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  submitted = false;
  loading = false;
  currentLang = 'en';
  isLanguageDropdownOpen = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private languageService: LanguageService,
    private translateService: TranslateService
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false]
    });

    // Initialize current language
    this.currentLang = this.languageService.getCurrentLanguage();

    // Subscribe to language changes
    this.languageService.currentLang$.subscribe(lang => {
      this.currentLang = lang;
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.authService.login({
      email: this.f['email'].value,
      password: this.f['password'].value,
      rememberMe: this.f['rememberMe'].value
    }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  /**
   * Change language
   */
  changeLanguage(lang: string): void {
    this.languageService.setLanguage(lang);
    this.isLanguageDropdownOpen = false;
  }

  /**
   * Toggle language dropdown
   */
  toggleLanguageDropdown(): void {
    this.isLanguageDropdownOpen = !this.isLanguageDropdownOpen;
  }

  /**
   * Close dropdown when clicking outside
   */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const dropdownElement = document.querySelector('.dropdown-language');
    if (dropdownElement && !dropdownElement.contains(target)) {
      this.isLanguageDropdownOpen = false;
    }
  }

  /**
   * Toggle between Arabic and English
   */
  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }

  /**
   * Get current language display name
   */
  getLanguageDisplay(): string {
    return this.languageService.getLanguageDisplay();
  }

  /**
   * Check if current language is RTL
   */
  isCurrentLangRTL(): boolean {
    return this.languageService.isRTL();
  }
}
