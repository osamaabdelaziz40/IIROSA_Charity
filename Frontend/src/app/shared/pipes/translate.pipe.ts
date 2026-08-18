import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'translate',
  standalone: true
})
export class TranslatePipe implements PipeTransform {
  transform(value: string): string {
    // For now, just return the value as-is
    // This can be integrated with ngx-translate later
    if (!value) return '';
    return value;
  }
}
