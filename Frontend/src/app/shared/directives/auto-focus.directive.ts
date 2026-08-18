import { Directive, Input, HostListener, ElementRef } from '@angular/core';

@Directive({
  selector: '[appAutoFocus]'
})
export class AutoFocusDirective {
  @Input() appAutoFocus: boolean = true;

  constructor(private el: ElementRef) {}

  ngAfterViewInit() {
    if (this.appAutoFocus) {
      setTimeout(() => {
        this.el.nativeElement.focus();
      }, 0);
    }
  }
}
