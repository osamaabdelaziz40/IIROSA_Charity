import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CharityDto } from '../../modules/charities/models/charity.model';

/**
 * Global "selected charity" state behind the header switcher.
 *
 * The header dropdown stores its choice here instead of navigating; screens
 * that care (currently the dashboard card) subscribe to selectedCharity$. The
 * choice is persisted to localStorage so it survives the full page reload the
 * language switcher performs. "All Charities" is stored as the sentinel id
 * 'all' with no charity attached.
 */
@Injectable({
  providedIn: 'root'
})
export class SelectedCharityService {
  private static readonly STORAGE_KEY = 'selectedCharity';

  private readonly stored = this.readStored();

  private selectedCharitySubject = new BehaviorSubject<CharityDto | null>(this.stored.charity);

  /** Emits the selected charity, or null when "All Charities"/nothing is chosen. */
  selectedCharity$: Observable<CharityDto | null> = this.selectedCharitySubject.asObservable();

  getSelectedCharity(): CharityDto | null {
    return this.selectedCharitySubject.value;
  }

  /**
   * The widget-facing selection: 'all' or the charity's id. Used to restore the
   * header select2 after a page reload.
   */
  getSelectedId(): string | null {
    return this.stored.id;
  }

  setCharity(charity: CharityDto): void {
    this.stored.id = charity.id;
    localStorage.setItem(SelectedCharityService.STORAGE_KEY, JSON.stringify({ id: charity.id, charity }));
    this.selectedCharitySubject.next(charity);
  }

  selectAll(): void {
    this.stored.id = 'all';
    localStorage.setItem(SelectedCharityService.STORAGE_KEY, JSON.stringify({ id: 'all', charity: null }));
    this.selectedCharitySubject.next(null);
  }

  clear(): void {
    this.stored.id = null;
    localStorage.removeItem(SelectedCharityService.STORAGE_KEY);
    this.selectedCharitySubject.next(null);
  }

  private readStored(): { id: string | null; charity: CharityDto | null } {
    try {
      const raw = localStorage.getItem(SelectedCharityService.STORAGE_KEY);
      const parsed = raw ? JSON.parse(raw) : null;
      if (parsed && typeof parsed.id === 'string') {
        return { id: parsed.id, charity: parsed.charity ?? null };
      }
    } catch (e) {
      // Corrupt payload — treat as no selection.
    }
    return { id: null, charity: null };
  }
}
