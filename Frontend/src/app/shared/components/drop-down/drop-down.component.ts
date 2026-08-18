import { Component, OnInit, Input, Output, EventEmitter, OnDestroy, OnChanges, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subscription, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import isRequired from '../../helpers/decorator';
import { LookupBase } from '../../models/lookup.base.model';
import { LookupService } from '../../services/lookup.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
declare var $: any;  // jQuery declaration for Select2

@Component({
  selector: 'app-drop-down',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './drop-down.component.html',
  styleUrls: ['./drop-down.component.scss']
})
export class DropDownComponent implements OnInit, OnChanges, OnDestroy, AfterViewInit {
  @Input() skipCaching: boolean = false;
  @isRequired()
  @Input() fg!: FormGroup
  @isRequired()
  @Input() fcn!: string
  @Input() url!: string
  @Input() initialData!: Array<LookupBase>
  @Input() label!: string
  @Input() placeholder!: string
  @Input() className!: string
  @Input() required: boolean = false
  @Input() disabled: boolean = false
  @Input() hasDefaultValue: boolean = true
  @Input() disableFirstValue: boolean = false
  @Input() helpText!: string
  @Input() helpTextClass: string = 'form-text text-muted'
  @Input() id!: string
  @Input() groupBy!: string  // Field to group options by
  @Input() multiple: boolean = false  // Enable multiple selection
  @Output() onDropDownValueChanged: EventEmitter<any> = new EventEmitter()
  @Output() onDropDownChanged: EventEmitter<any> = new EventEmitter()
  @Input() valueName!: string
  @ViewChild('selectElement', { static: false }) selectElement!: ElementRef;

  subscribe$!: Subject<any>
  subscription!: Subscription
  data: Array<LookupBase> = []
  groupedData: Array<{ group?: string; items: Array<LookupBase> }> = []
  select2Instance: any = null
  isRequired: boolean = false
  private previousDisabledState: boolean | null = null
  private isInitializing: boolean = false
  private select2UnavailableWarningShown: boolean = false

  constructor(private lookupService: LookupService, private el: ElementRef, private translate: TranslateService) {
  }

  ngOnChanges() {
    // Skip changes during initialization to prevent loops
    if (this.isInitializing) {
      return
    }

    // Handle disabled state change - only when it actually changes
    if (this.previousDisabledState !== this.disabled) {
      this.updateDisabledState()
      this.previousDisabledState = this.disabled
    }

    if (this.initialData) {
      this.data = this.initialData
      this.processGroupedData()
      // Re-initialize Select2 when data changes, but only if view is ready
      // Use a flag to ensure we don't initialize before view is ready
      setTimeout(() => {
        this.isInitializing = true
        if (this.select2Instance) {
          this.destroySelect2()
        }
        this.initSelect2()
        setTimeout(() => {
          this.isInitializing = false
        }, 150)
      }, 50)
    }
  }

  private updateDisabledState() {
    const control = this.fg.get(this.fcn)
    if (control) {
      const currentlyDisabled = control.disabled
      // Only update if the state actually differs
      if (this.disabled && !currentlyDisabled) {
        control.disable({ emitEvent: false })
      } else if (!this.disabled && currentlyDisabled) {
        control.enable({ emitEvent: false })
      }
      // Update Select2 disabled state if already initialized
      if (this.select2Instance) {
        const selectElement = $(this.el.nativeElement).find('select.select2')
        if (this.disabled) {
          this.select2Instance.prop('disabled', true)
          selectElement.trigger('change.select2')
        } else {
          this.select2Instance.prop('disabled', false)
          selectElement.trigger('change.select2')
        }
      }
    }
  }

  private emitSelectedValue(id: any) {
    // Fix: Check for null/undefined explicitly, not falsy values (0 is valid!)
    if (id === null || id === undefined || id === '') {
      this.onDropDownChanged.emit(null)
      return
    }
    // Handle type mismatch: Select2 returns string, but data IDs might be numbers
    const value = this.data.find(x => x.id == id || String(x.id) === String(id))
    if (value) {
      this.onDropDownChanged.emit(value)
    } else {
      // Emit the id directly if no matching item found
      this.onDropDownChanged.emit({ id: id })
    }
  }

  private emitSelectedValues(ids: any[]) {
    if (!ids || ids.length === 0) {
      this.onDropDownChanged.emit([])
      return
    }
    const values = ids.map(id => {
      const value = this.data.find(x => x.id == id || String(x.id) === String(id))
      return value || { id: id }
    })
    this.onDropDownChanged.emit(values)
  }

  ngOnInit() {
    this.isRequired = this.hasRequiredValidator();
    this.valueName = this.valueName ? this.valueName : 'name'
    this.skipCaching = this.skipCaching
    this.observe()
    // Set initial disabled state and track it
    this.previousDisabledState = this.disabled
    this.updateDisabledState()
    if (this.url) {
      this.fetchUrl();
    } else if (this.initialData) {
      this.data = this.initialData
      this.processGroupedData()
    }
  }

  ngAfterViewInit() {
    // Only initialize Select2 if we have data
    if (this.data && this.data.length > 0) {
      this.initSelect2()
    }
  }

  private initSelect2() {
    // Wait for Angular to finish rendering
    setTimeout(() => {
      // Check if jQuery and Select2 are available
      if (typeof $ === 'undefined' || !$.fn.select2) {
        if (!this.select2UnavailableWarningShown) {
          console.warn('[DropDown] jQuery or Select2 is not loaded. Dropdowns will use native select elements.')
          this.select2UnavailableWarningShown = true
        }
        return
      }

      // Only initialize if we have data
      if (!this.data || this.data.length === 0) {
        return
      }

      const selectElement = $(this.el.nativeElement).find('select.select2')
      if (selectElement.length) {
        // Always destroy and re-create to ensure fresh event handlers
        if (this.select2Instance) {
          this.select2Instance.select2('destroy')
          this.select2Instance = null
        }

        // Initialize Select2 with Bootstrap 4 theme
        this.select2Instance = selectElement.select2({
          theme: 'bootstrap4',
          width: '100%',
          dropdownParent: $(this.el.nativeElement),
          placeholder: this.placeholder || this.translate.instant('validation.selectAnOption'),
          multiple: this.multiple,
          closeOnSelect: !this.multiple  // Keep dropdown open for multi-select
        })

        // Handle Select2 select event (after selection is complete)
        this.select2Instance.on('select2:select', (e: any) => {
          const selectedData = e.params.data
          const control = this.fg.get(this.fcn)
          if (control) {
            // Ensure ID is clean (no whitespace, proper type)
            const cleanId = typeof selectedData.id === 'string' ? parseInt(selectedData.id.trim(), 10) : selectedData.id
            if (this.multiple) {
              // For multi-select, add to array
              const currentValue = control.value || []
              const newValue = [...currentValue, cleanId]
              control.setValue(newValue)
              control.markAsDirty()
              this.emitSelectedValues(newValue)
            } else {
              control.setValue(cleanId)
              control.markAsDirty()
              this.emitSelectedValue(cleanId)
            }
          }
        })

        // Handle Select2 unselect event
        this.select2Instance.on('select2:unselect', (e: any) => {
          const control = this.fg.get(this.fcn)
          if (control && this.multiple) {
            const currentValue = control.value || []
            // Ensure clean ID for comparison
            const unselectedId = typeof e.params.data.id === 'string' ? parseInt(e.params.data.id.trim(), 10) : e.params.data.id
            const newValue = currentValue.filter((id: any) => {
              const cleanId = typeof id === 'string' ? parseInt(id.trim(), 10) : id
              return cleanId !== unselectedId
            })
            control.setValue(newValue)
            control.markAsDirty()
            this.emitSelectedValues(newValue)
          } else {
            this.onDropDownChanged.emit(null)
          }
        })

        // Update Angular form control when Select2 changes
        this.select2Instance.on('change', (e: any) => {
          if (this.multiple) {
            // For multi-select, value is already handled by select/unselect events
            return
          }
          // Don't set the control value here - it's already set by select2:select event
          // Just mark as dirty and emit for cascading dropdowns
          const control = this.fg.get(this.fcn)
          if (control) {
            control.markAsDirty()
            // Emit the current control value (not e.target.value which is a string)
            this.emitSelectedValue(control.value)
          }
        })

        // Sync Angular form value with Select2
        const control = this.fg.get(this.fcn)
        if (control && control.value) {
          if (this.multiple && Array.isArray(control.value)) {
            selectElement.val(control.value).trigger('change.select2')
          } else if (!this.multiple) {
            selectElement.val(control.value).trigger('change.select2')
          }
        }
      }
    }, 100)
  }

  private destroySelect2() {
    if (this.select2Instance) {
      try {
        // Check if the element still exists in DOM and has Select2 initialized
        if (this.select2Instance.length && this.select2Instance.data('select2')) {
          this.select2Instance.select2('destroy')
        }
      } catch (error) {
        console.warn('[DropDown] Select2 destroy error (element may already be destroyed):', error)
      } finally {
        this.select2Instance = null
      }
    }
  }

  private processGroupedData() {
    if (this.groupBy && this.data.length > 0) {
      // Group data by the specified field
      const groups = new Map<string, Array<LookupBase>>()
      this.data.forEach(item => {
        const groupKey = (item as any)[this.groupBy] || 'Other'
        if (!groups.has(groupKey)) {
          groups.set(groupKey, [])
        }
        groups.get(groupKey)!.push(item)
      })

      this.groupedData = Array.from(groups.entries()).map(([group, items]) => ({
        group,
        items
      }))
    } else {
      // No grouping, put all items in a single group
      this.groupedData = [{ items: this.data }]
    }
  }

  fetchUrl() {
    this.lookupService.getLookup(this.url)
      .pipe(map((res): Array<LookupBase> => {
        !this.skipCaching && this.lookupService.saveLookup(this.url, res)
        const responseValue = res.value;
        if (!responseValue) return [];
        return (responseValue).map((obj: any) => {
          return {
            id: obj.id,
            name: obj[this.valueName as keyof typeof obj],
            [this.valueName]: obj[this.valueName as keyof typeof obj],
            systemValue: obj.systemValue,
            disabled: obj.disabled || false
          }
        })
      }))
      .subscribe(_res => {
        this.data = _res as Array<LookupBase>
        this.processGroupedData()
        const control = this.fg.get(this.fcn);
        if (control) {
          this.onChange(control.value)
        }
        // Re-initialize Select2 after data loads
        this.destroySelect2()
        this.initSelect2()
      })
  }

  observe() {
    const control = this.fg.get(this.fcn);
    if (control) {
      this.subscription = control.valueChanges
        .subscribe(_res => {
          const ctrl = this.fg.get(this.fcn);
          if (ctrl) {
            ctrl.updateValueAndValidity({ emitEvent: false })
          }
          this.onChange(_res)
          // Sync Select2 with form value
          if (this.select2Instance) {
            const selectElement = $(this.el.nativeElement).find('select.select2')
            if (this.multiple && Array.isArray(_res)) {
              selectElement.val(_res).trigger('change.select2')
            } else if (!this.multiple) {
              selectElement.val(_res).trigger('change.select2')
            }
          }
        })
    }
  }

  onChange(id: any) {
    const value = this.data.find(x => x.id == id)
    if (value) {
      this.onDropDownValueChanged.emit(value)
    }
  }

  dropDownChanged(event: any) {
    const rawValue = event.target.value
    const value = this.data.find(x => x.id == rawValue || String(x.id) === String(rawValue))
    if (value) {
      this.onDropDownChanged.emit(value)
    } else {
      // If not found in data, emit the raw ID wrapped in an object
      const cleanId = typeof rawValue === 'string' ? parseInt(rawValue.trim(), 10) : rawValue
      this.onDropDownChanged.emit({ id: cleanId })
    }
  }

  getItemValue(item: LookupBase): any {
    return item[(this.valueName as keyof LookupBase)];
  }

  isFieldInvalid(): boolean {
    const field = this.fg.get(this.fcn);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  hasRequiredValidator(): boolean {
    const control = this.fg.get(this.fcn);
    if (!control || !control.validator) {
      return false;
    }
    const validator = control.validator({} as any);
    return validator && validator['required'];
  }

  isFieldValid(): boolean {
    const field = this.fg.get(this.fcn);
    return !!(field && field.valid && (field.dirty || field.touched));
  }

  getErrorMessage(): string {
    const field = this.fg.get(this.fcn);
    if (field && field.errors) {
      if (field.errors['required']) {
        return 'validation.required';
      }
    }
    return '';
  }

  ngOnDestroy() {
    this.destroySelect2()
    this.subscription && this.subscription.unsubscribe()
  }
}
