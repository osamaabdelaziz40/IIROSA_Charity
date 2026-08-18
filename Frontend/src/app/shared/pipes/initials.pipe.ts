import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'initials'
})
export class InitialsPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';

    const parts = value.trim().split(' ');
    if (parts.length === 0) return '';

    // Get first letters of each word
    const initials = parts.map(part => {
      const word = part.trim();
      if (word.length === 0) return '';
      return word[0].toUpperCase();
    });

    // Limit to 2 initials
    return initials.slice(0, 2).join('');
  }
}
