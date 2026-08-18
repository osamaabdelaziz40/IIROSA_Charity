import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'appDate',
  standalone: true
})
export class AppDatePipe implements PipeTransform {
  private datePipe = new DatePipe('en-US');

  transform(value: Date | string, format: string = 'mediumDate'): string {
    if (!value) return '';
    return this.datePipe.transform(value, format) || '';
  }
}
