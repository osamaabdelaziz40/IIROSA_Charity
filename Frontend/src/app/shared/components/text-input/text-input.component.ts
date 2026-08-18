import { Component, OnInit, Input, Output, OnDestroy, EventEmitter, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import isRequired from '../../helpers/decorator';
import { TranslateService } from '@ngx-translate/core';

/**
 * Supported input types:
 * - text, password, email, tel, number, url, search
 * - date, time, datetime-local, month, week
 * - color, range, file, image
 * - textarea (special handling)
 * - list (uses ng-content for custom options)
 * - date-picker (bootstrap datepicker with special handling)
 */
type InputType = 'text' | 'password' | 'email' | 'tel' | 'number' | 'url' | 'search' |
                  'date' | 'time' | 'datetime-local' | 'month' | 'week' |
                  'color' | 'range' | 'file' | 'image' |
                  'textarea' | 'list' | 'date-picker';

@Component({
  selector: 'app-input-text',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class InputTextComponent implements OnInit, OnDestroy, OnChanges {

  @isRequired()
  @Input() fg!: FormGroup;
  @isRequired()
  @Input() fcn!: string;
  @Input() className!: string;
  @Input() placeholder!: string;
  @Input() label!: string;
  @Input() isPassword!: boolean;
  @Input() autoComplete!: string;
  @Input() type: InputType = 'text';
  @Input() value!: any;
  @Input() id!: string;
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;
  @Input() readonly: boolean = false;
  @Input() helpText!: string;
  @Input() rows: number = 3;
  @Input() helpTextClass: string = 'text-muted';

  // Date picker specific
  @Input() minDate!: Date;
  @Input() maxDate!: Date;
  @Input() editDate: boolean = false;

  // HTML5 input attributes
  @Input() min!: string | number;
  @Input() max!: string | number;
  @Input() step!: string | number;
  @Input() pattern!: string;
  @Input() inputMode!: string;
  @Input() multiple!: boolean;
  @Input() accept!: string; // For file inputs

  // Bootstrap datepicker config
  @Input() bsConfig: any = {};

  // Output events
  @Output() input = new EventEmitter<any>();
  @Output() change = new EventEmitter<any>();
  @Output() focus = new EventEmitter<any>();
  @Output() blur = new EventEmitter<any>();

  subscribe = new Subject<void>();
  minLength: any = {};
  show: boolean = false;
  isRequired: boolean = false;

  constructor(private translate: TranslateService) {
    // Subscribe to language changes to trigger re-render
    this.translate.onLangChange.subscribe(() => {
      // Force update by marking for check
      this.show = false;
      setTimeout(() => this.show = true);
    });
  }

  ngOnInit(): void {
    this.isRequired = this.hasRequiredValidator();
    this.mainTask();
    this.show = true;
    // Set initial disabled state
    this.updateDisabledState();

    // Initialize date value if type is date-picker
    if (this.type === 'date-picker') {
      this.initializeDateValue();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Handle disabled state changes
    if (changes['disabled'] && !changes['disabled'].firstChange) {
      this.updateDisabledState();
    }
  }

  private updateDisabledState(): void {
    const control = this.fg.get(this.fcn);
    if (control) {
      const currentlyDisabled = control.disabled;
      // Only update if the state actually differs
      if (this.disabled && !currentlyDisabled) {
        control.disable({ emitEvent: false });
      } else if (!this.disabled && currentlyDisabled) {
        control.enable({ emitEvent: false });
      }
    }
  }

  private initializeDateValue(): void {
    const control = this.fg.get(this.fcn);
    if (control && control.value) {
      // Ensure the date value is a valid Date object
      const dateValue = control.value;
      if (typeof dateValue === 'string') {
        // Parse string date
        const parsedDate = new Date(dateValue);
        if (!isNaN(parsedDate.getTime())) {
          control.setValue(parsedDate, { emitEvent: false });
        }
      } else if (!(dateValue instanceof Date)) {
        // Convert to Date if it's not already
        control.setValue(new Date(dateValue), { emitEvent: false });
      }
    }
  }

  mainTask(): void {
    const control = this.fg.get(this.fcn);
    if (control) {
      control.valueChanges
        .pipe(takeUntil(this.subscribe))
        .subscribe(_res => {
          const ctrl = this.fg.get(this.fcn);
          const err = ctrl?.errors;
          const minLengthErr = err ? err['minlength'] : '';
          this.minLength = { key: ':len', value: minLengthErr ? minLengthErr.requiredLength : '' };
        });
    }
  }

  onClick(): void {
    // Can be used to enable readonly fields on click
  }

  onInput(event: any): void {
    this.input.emit(event);
  }

  onChange(event: any): void {
    this.change.emit(event);
  }

  onFocus(event: any): void {
    this.focus.emit(event);
  }

  onBlur(event: any): void {
    this.blur.emit(event);
  }

  /**
   * Determines if the current type is a standard input (not textarea, list, or date-picker)
   */
  isStandardInput(): boolean {
    return !['textarea', 'list', 'date-picker'].includes(this.type);
  }

  /**
   * Gets the actual input type attribute value
   */
  getInputType(): string {
    if (this.isPassword) return 'password';
    if (this.type === 'date-picker') return 'text';

    // Map our custom types to HTML5 types
    const typeMap: Record<string, string> = {
      'tel': 'tel',
      'email': 'email',
      'number': 'number',
      'url': 'url',
      'search': 'search',
      'date': 'date',
      'time': 'time',
      'datetime-local': 'datetime-local',
      'month': 'month',
      'week': 'week',
      'color': 'color',
      'range': 'range',
      'file': 'file',
      'image': 'image'
    };

    return typeMap[this.type] || 'text';
  }

  /**
   * Check if field has required validator
   */
  hasRequiredValidator(): boolean {
    const control = this.fg.get(this.fcn);
    if (!control || !control.validator) {
      return false;
    }
    // Check if the control has the required validator
    const validator = control.validator({} as any);
    return validator && validator['required'];
  }

  /**
   * Check if field is invalid
   */
  isFieldInvalid(): boolean {
    const field = this.fg.get(this.fcn);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Check if field is valid
   */
  isFieldValid(): boolean {
    const field = this.fg.get(this.fcn);
    return !!(field && field.valid && (field.dirty || field.touched));
  }

  /**
   * Get error message for current field errors
   */
  getErrorMessage(): string {
    const field = this.fg.get(this.fcn);
    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return this.translate.instant('validation.required');
    }
    if (field.errors['email']) {
      return this.translate.instant('validation.email');
    }
    if (field.errors['minlength']) {
      const requiredLength = field.errors['minlength'].requiredLength;
      return this.translate.instant('validation.minLength', { minLength: requiredLength });
    }
    if (field.errors['maxlength']) {
      const requiredLength = field.errors['maxlength'].requiredLength;
      return this.translate.instant('validation.maxLength', { maxLength: requiredLength });
    }
    if (field.errors['pattern']) {
      return this.translate.instant('validation.pattern');
    }
    if (field.errors['min']) {
      const minValue = field.errors['min'].min;
      return this.translate.instant('validation.min', { min: minValue });
    }
    if (field.errors['max']) {
      const maxValue = field.errors['max'].max;
      return this.translate.instant('validation.max', { max: maxValue });
    }
    if (field.errors['url']) {
      return this.translate.instant('validation.url');
    }

    return this.translate.instant('validation.invalid');
  }

  /**
   * Get default bsConfig for date picker
   */
  getBsConfig(): any {
    return {
      isAnimated: true,
      dateInputFormat: 'MM/DD/YYYY',
      containerClass: 'theme-blue',
      showWeekNumbers: false,
      adaptivePosition: true,
      rangeSeparator: '-',
      ...this.bsConfig
    };
  }

  /**
   * Handle date value change from Bootstrap datepicker
   */
  onDateValueChange(date: Date): void {
    if (date && this.fg && this.fcn) {
      const control = this.fg.get(this.fcn);
      if (control) {
        // Ensure the date is properly set as a Date object
        control.setValue(date, { emitEvent: false });
        control.markAsDirty();
        control.updateValueAndValidity();
      }
    }
  }

  /**
   * Open the datepicker programmatically
   */
  openDatePicker(): void {
    // This method can be used to trigger the datepicker open
    // The Bootstrap datepicker will handle the opening automatically
  }

  ngOnDestroy(): void {
    this.subscribe.next();
    this.subscribe.unsubscribe();
  }
}
