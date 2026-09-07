import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { NotificationService } from '../../../core/services/notification.service';
import { ReportPdfService, ReportFormSection } from '../services/report-pdf.service';
import {
  QuestionnaireVariant,
  buildSurveyQuestionnaire
} from './questionnaire-document';

/**
 * UC-RPT-26 (§23.U.26 طباعة الاستبانة) — the blank field-survey questionnaire.
 *
 * A STATIC document, not a data report: two command buttons produce the family
 * variant (استبانة الأسرة) or the widow variant (نسخة الأرامل) through 18-21's
 * browser-print service; no filters, no endpoint, nothing stored changes. The
 * document definition lives in `questionnaire-document.ts` (data-driven, i18n-keyed)
 * so 18-40's generic viewer can reuse it.
 */
@Component({
  selector: 'app-survey-questionnaire',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './survey-questionnaire.component.html',
  styleUrls: ['./survey-questionnaire.component.scss']
})
export class SurveyQuestionnaireComponent {
  /** window.print() blocks the main thread while the dialog is open — double-guard. */
  printing = false;

  private readonly i18nPrefix = 'reports.surveyQuestionnaire';

  constructor(
    private pdf: ReportPdfService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {}

  /** طباعة الاستبانة — the family variant, the form's default command (§23.U.26). */
  printFamily(): void {
    this.print('family');
  }

  /** نسخة الأرامل — the widow variant with the widow question set swapped in. */
  printWidow(): void {
    this.print('widow');
  }

  private print(variant: QuestionnaireVariant): void {
    if (this.printing) {
      return;
    }
    this.printing = true;
    // Review P27 2026-08-26: window.print() blocks the main thread — deferring one tick
    // lets the [disabled]="printing" double-click guard actually paint (set+clear in one
    // tick never rendered, so the guard was illusory).
    setTimeout(() => {
      try {
        const questionnaire = buildSurveyQuestionnaire(variant);
        const sections: ReportFormSection[] = questionnaire.sections.map(section => ({
          title: this.label(`sections.${section.titleKey}`),
          ...(section.fields
            ? { fields: section.fields.map(field => ({ label: this.label(`fields.${field.labelKey}`) })) }
            : {}),
          ...(section.grid
            ? {
                grid: {
                  headers: section.grid.columnKeys.map(key => this.label(`fields.${key}`)),
                  rowCount: section.grid.rowCount
                }
              }
            : {}),
          ...(section.noteLines ? { noteLines: section.noteLines } : {}),
          ...(section.signatureKeys
            ? { signatureSlots: section.signatureKeys.map(key => this.label(`signatures.${key}`)) }
            : {})
        }));

        const title = this.label(`${questionnaire.titleKey}`);
        const printedOn = new Date().toISOString().slice(0, 10);
        this.pdf.printForm({
          documentTitle: title,
          title,
          subtitle: this.label('docSubtitle'),
          sections,
          footer: `${title} — ${this.label('footerDate')}: ${printedOn}`
        });
      } catch (error) {
        // AC 5 — never a silent no-op. The browser-print path has no font to load
        // (18-21's decision), so the classic font-failure fallback does not apply;
        // any composition failure still surfaces as a toast.
        console.error('Survey questionnaire print failed', error);
        this.notification.error(this.label('printFailed'));
      } finally {
        this.printing = false;
      }
    }, 50);
  }

  private label(key: string): string {
    return this.translate.instant(`${this.i18nPrefix}.${key}`);
  }
}
