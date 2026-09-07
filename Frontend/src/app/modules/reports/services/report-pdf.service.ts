import { Injectable } from '@angular/core';

/** One printable column: the header label and the cell renderer (plain text). */
export interface ReportSheetColumn<T> {
  header: string;
  render: (row: T, index: number) => string;
}

/** A label/value pair rendered above the table (batch no, date, charity scope…). */
export interface ReportSheetMeta {
  label: string;
  value: string;
}

/** One print job — heading, meta rows, columns, and the data rows. */
export interface ReportSheetConfig<T> {
  /** The browser's print dialog suggests this as the file name (document.title during print). */
  documentTitle: string;
  /** The sheet heading. */
  title: string;
  subtitle?: string;
  meta?: ReportSheetMeta[];
  columns: ReportSheetColumn<T>[];
  rows: T[];
}

/** A labelled blank line on a printed form (questionnaire field — no data). */
export interface ReportFormField {
  label: string;
}

/** One form section: heading plus any mix of blank lines, a blank grid, notes, signatures. */
export interface ReportFormSection {
  title: string;
  fields?: ReportFormField[];
  /** Repeated fully-blank rows under the given headers (بيانات الأيتام…). */
  grid?: { headers: string[]; rowCount: number };
  /** Ruled free lines (ملاحظات). */
  noteLines?: number;
  /** Signature slot labels rendered under the ruled line (توقيعات). */
  signatureSlots?: string[];
}

/**
 * A form-shaped print job — the blank questionnaire (18-26 استبانة) rather than a
 * data table. Same pipeline, cleanup and document-title swap as a sheet.
 */
export interface ReportFormConfig {
  documentTitle: string;
  title: string;
  subtitle?: string;
  sections: ReportFormSection[];
  /** Small centred line under the last section (document identity + print date). */
  footer?: string;
}

/** One labelled line inside a printed card (اسم الجمعية · رقم اليتيم …). */
export interface ReportCardField<T> {
  header: string;
  render: (row: T) => string;
}

/**
 * A card-sheet print job — repeated fixed-size cards on A4 with cut guides between them
 * (18-31's كروت الاستلام, 18-34's كروت الأسر). Each card renders the field lines for one
 * row plus a signature ruled line; `break-inside: avoid` keeps every card on one page.
 */
export interface ReportCardSheetConfig<T> {
  /** The browser's print dialog suggests this as the file name (document.title during print). */
  documentTitle: string;
  title: string;
  subtitle?: string;
  meta?: ReportSheetMeta[];
  fields: ReportCardField<T>[];
  /** The label under the signature ruled line (توقيع المستلم …); omit for no line. */
  signatureLabel?: string;
  rows: T[];
}

/**
 * A previewable document (18-40 عرض التقرير) — a complete standalone RTL HTML document built
 * by the SAME renderer that prints (no parallel pipeline). The preview modal frames it via a
 * blob URL; طباعة runs the frame's native print; حفظ re-uses the same blob.
 */
export interface ReportPreviewSource {
  /** Suggested file name for حفظ (and the framed document's `<title>`). */
  documentTitle: string;
  /** The full standalone document — `<!DOCTYPE html>` … `</html>`. */
  html: string;
}

/**
 * EP-18's PDF producer — **the epic-wide PDF decision (18-21, decide-once)**:
 *
 * Arabic is produced via the **browser's print pipeline**, not a JS PDF library.
 * jsPDF was investigated and rejected: it performs no OpenType/Arabic shaping — text
 * renders as isolated, unjoined letterforms even with an embedded Amiri/Cairo TTF,
 * and the pre-shape/reverse-BIDI workarounds break on the mixed Arabic/numeric/LTR
 * content these sheets carry (codes, dates, counts). The browser's native shaping
 * engine renders the Arabic correctly, `direction: rtl` gives the correct column
 * order, and "Save as PDF" in the print dialog produces the file. No font asset to
 * license or commit; no CDN fetch at print time.
 *
 * Every print story (18-21, 18-26, 18-29 … 18-37) builds on this service — none
 * re-investigates the library choice.
 */
@Injectable({ providedIn: 'root' })
export class ReportPdfService {
  private static readonly STYLE_ID = 'report-print-sheet-styles';
  private static readonly SHEET_CLASS = 'report-print-sheet';
  private static readonly FORM_CLASS = 'report-print-form';
  private static readonly CARDS_CLASS = 'report-print-cards';

  /**
   * Renders the sheet into a hidden RTL container and opens the browser print
   * dialog. Callers decide emptiness policy before calling — an empty `rows`
   * array prints a header-only sheet, so the nothing-to-produce guard lives in
   * the screens, not here.
   */
  printSheet<T>(config: ReportSheetConfig<T>): void {
    this.ensureStyles();

    const sheet = this.buildSheetElement(config);

    // The print dialog suggests document.title as the file name.
    const previousTitle = document.title;
    document.title = config.documentTitle;
    document.body.appendChild(sheet);

    const cleanup = (): void => {
      sheet.remove();
      document.title = previousTitle;
      window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    try {
      window.print();
    } catch {
      // Review P18 2026-08-26: if the dialog never opens (print blocked or threw),
      // afterprint never fires — cleanup here so the hidden container and the swapped
      // document.title never leak past the failure. Idempotent with the listener path.
      cleanup();
    }
  }

  /**
   * The previewable sibling of `printSheet` (18-40 عرض) — the SAME sheet config becomes a
   * standalone RTL document for the preview modal's blob-URL iframe. No print dialog here;
   * the modal owns طباعة/حفظ over the framed document.
   */
  sheetSource<T>(config: ReportSheetConfig<T>): ReportPreviewSource {
    return this.standaloneDocument(this.buildSheetElement(config), config.documentTitle);
  }

  /** The sheet's DOM — shared verbatim by the print path and the preview path. */
  private buildSheetElement<T>(config: ReportSheetConfig<T>): HTMLDivElement {
    const sheet = document.createElement('div');
    sheet.className = ReportPdfService.SHEET_CLASS;
    sheet.setAttribute('dir', 'rtl');

    const title = document.createElement('h1');
    title.className = 'report-print-sheet__title';
    title.textContent = config.title;
    sheet.appendChild(title);

    if (config.subtitle) {
      const subtitle = document.createElement('div');
      subtitle.className = 'report-print-sheet__subtitle';
      subtitle.textContent = config.subtitle;
      sheet.appendChild(subtitle);
    }

    if (config.meta && config.meta.length > 0) {
      const meta = document.createElement('div');
      meta.className = 'report-print-sheet__meta';
      for (const pair of config.meta) {
        const item = document.createElement('span');
        item.className = 'report-print-sheet__meta-item';
        const label = document.createElement('strong');
        label.textContent = `${pair.label}: `;
        item.appendChild(label);
        item.appendChild(document.createTextNode(pair.value));
        meta.appendChild(item);
      }
      sheet.appendChild(meta);
    }

    const table = document.createElement('table');
    const thead = document.createElement('thead');
    const headRow = document.createElement('tr');
    for (const column of config.columns) {
      const th = document.createElement('th');
      th.setAttribute('scope', 'col');
      th.textContent = column.header;
      headRow.appendChild(th);
    }
    thead.appendChild(headRow);
    table.appendChild(thead);

    const tbody = document.createElement('tbody');
    config.rows.forEach((row, index) => {
      const tr = document.createElement('tr');
      for (const column of config.columns) {
        const td = document.createElement('td');
        // textContent only — cell values are data, never markup.
        td.textContent = column.render(row, index);
        tr.appendChild(td);
      }
      tbody.appendChild(tr);
    });
    table.appendChild(tbody);
    sheet.appendChild(table);

    return sheet;
  }

  /**
   * The form-shaped sibling of `printSheet` — same browser-print pipeline, cleanup
   * and document-title swap, but renders a blank questionnaire (section headings,
   * ruled blank lines, repeated blank-row grids, notes lines, signature slots)
   * instead of a data table. 18-26's استبانة is the first caller; the shape stays
   * data-driven so later print stories reuse it.
   *
   * Page numbers: Chromium's print engine does not implement `@page` margin-box
   * counters, so the printed page numbers come from the browser's native print
   * headers/footers; the form itself carries a footer line instead.
   */
  printForm(config: ReportFormConfig): void {
    this.ensureStyles();

    const form = this.buildFormElement(config);

    const previousTitle = document.title;
    document.title = config.documentTitle;
    document.body.appendChild(form);

    const cleanup = (): void => {
      form.remove();
      document.title = previousTitle;
      window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    try {
      window.print();
    } catch {
      // Review P18 2026-08-26: if the dialog never opens (print blocked or threw),
      // afterprint never fires — cleanup here so the hidden container and the swapped
      // document.title never leak past the failure. Idempotent with the listener path.
      cleanup();
    }
  }

  /**
   * The previewable sibling of `printForm` (18-40 عرض) — the blank questionnaire as a
   * standalone RTL document for the preview modal's blob-URL iframe.
   */
  formSource(config: ReportFormConfig): ReportPreviewSource {
    return this.standaloneDocument(this.buildFormElement(config), config.documentTitle);
  }

  /** The form's DOM — shared verbatim by the print path and the preview path. */
  private buildFormElement(config: ReportFormConfig): HTMLDivElement {
    const form = document.createElement('div');
    form.className = ReportPdfService.FORM_CLASS;
    form.setAttribute('dir', 'rtl');

    const title = document.createElement('h1');
    title.className = 'report-print-form__title';
    title.textContent = config.title;
    form.appendChild(title);

    if (config.subtitle) {
      const subtitle = document.createElement('div');
      subtitle.className = 'report-print-form__subtitle';
      subtitle.textContent = config.subtitle;
      form.appendChild(subtitle);
    }

    for (const section of config.sections) {
      const box = document.createElement('section');
      box.className = 'report-print-form__section';

      const heading = document.createElement('div');
      heading.className = 'report-print-form__section-title';
      heading.textContent = section.title;
      box.appendChild(heading);

      for (const field of section.fields || []) {
        const row = document.createElement('div');
        row.className = 'report-print-form__field';
        const label = document.createElement('span');
        label.className = 'report-print-form__field-label';
        label.textContent = `${field.label}: `;
        const line = document.createElement('span');
        line.className = 'report-print-form__field-line';
        row.appendChild(label);
        row.appendChild(line);
        box.appendChild(row);
      }

      if (section.grid) {
        const table = document.createElement('table');
        const thead = document.createElement('thead');
        const headRow = document.createElement('tr');
        for (const header of section.grid.headers) {
          const th = document.createElement('th');
          th.setAttribute('scope', 'col');
          th.textContent = header;
          headRow.appendChild(th);
        }
        thead.appendChild(headRow);
        table.appendChild(thead);

        const tbody = document.createElement('tbody');
        for (let rowNumber = 0; rowNumber < section.grid.rowCount; rowNumber++) {
          const tr = document.createElement('tr');
          for (let column = 0; column < section.grid.headers.length; column++) {
            const td = document.createElement('td');
            // Non-breaking space keeps the ruled cell height on a blank row.
            td.textContent = ' ';
            tr.appendChild(td);
          }
          tbody.appendChild(tr);
        }
        table.appendChild(tbody);
        box.appendChild(table);
      }

      for (let line = 0; line < (section.noteLines || 0); line++) {
        const noteLine = document.createElement('div');
        noteLine.className = 'report-print-form__note-line';
        box.appendChild(noteLine);
      }

      if (section.signatureSlots && section.signatureSlots.length > 0) {
        const signatures = document.createElement('div');
        signatures.className = 'report-print-form__signatures';
        for (const slot of section.signatureSlots) {
          const signature = document.createElement('div');
          signature.className = 'report-print-form__signature';
          const ruledLine = document.createElement('div');
          ruledLine.className = 'report-print-form__signature-line';
          const label = document.createElement('span');
          label.textContent = slot;
          signature.appendChild(ruledLine);
          signature.appendChild(label);
          signatures.appendChild(signature);
        }
        box.appendChild(signatures);
      }

      form.appendChild(box);
    }

    if (config.footer) {
      const footer = document.createElement('div');
      footer.className = 'report-print-form__footer';
      footer.textContent = config.footer;
      form.appendChild(footer);
    }

    return form;
  }

  /**
   * The card-sheet sibling — repeated fixed-size cards on A4 with cut guides (dashed
   * borders), one card per row, `break-inside: avoid` so no card splits across pages.
   * 18-31's كروت الاستلام is the first caller; 18-34's كروت الأسر reuses it. Callers
   * decide emptiness policy before calling — the guard lives in the screens.
   */
  printCardSheet<T>(config: ReportCardSheetConfig<T>): void {
    this.ensureStyles();

    const sheet = this.buildCardsElement(config);

    const previousTitle = document.title;
    document.title = config.documentTitle;
    document.body.appendChild(sheet);

    const cleanup = (): void => {
      sheet.remove();
      document.title = previousTitle;
      window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    try {
      window.print();
    } catch {
      // Review P18 2026-08-26: if the dialog never opens (print blocked or threw),
      // afterprint never fires — cleanup here so the hidden container and the swapped
      // document.title never leak past the failure. Idempotent with the listener path.
      cleanup();
    }
  }

  /**
   * The previewable sibling of `printCardSheet` (18-40 عرض) — the card sheet as a standalone
   * RTL document for the preview modal's blob-URL iframe.
   */
  cardsSource<T>(config: ReportCardSheetConfig<T>): ReportPreviewSource {
    return this.standaloneDocument(this.buildCardsElement(config), config.documentTitle);
  }

  /** The cards' DOM — shared verbatim by the print path and the preview path. */
  private buildCardsElement<T>(config: ReportCardSheetConfig<T>): HTMLDivElement {
    const sheet = document.createElement('div');
    sheet.className = ReportPdfService.CARDS_CLASS;
    sheet.setAttribute('dir', 'rtl');

    const title = document.createElement('h1');
    title.className = 'report-print-cards__title';
    title.textContent = config.title;
    sheet.appendChild(title);

    if (config.subtitle) {
      const subtitle = document.createElement('div');
      subtitle.className = 'report-print-cards__subtitle';
      subtitle.textContent = config.subtitle;
      sheet.appendChild(subtitle);
    }

    if (config.meta && config.meta.length > 0) {
      const meta = document.createElement('div');
      meta.className = 'report-print-cards__meta';
      for (const pair of config.meta) {
        const item = document.createElement('span');
        item.className = 'report-print-cards__meta-item';
        const label = document.createElement('strong');
        label.textContent = `${pair.label}: `;
        item.appendChild(label);
        item.appendChild(document.createTextNode(pair.value));
        meta.appendChild(item);
      }
      sheet.appendChild(meta);
    }

    const grid = document.createElement('div');
    grid.className = 'report-print-cards__grid';
    for (const row of config.rows) {
      const card = document.createElement('div');
      card.className = 'report-print-cards__card';

      for (const field of config.fields) {
        const line = document.createElement('div');
        line.className = 'report-print-cards__field';
        const label = document.createElement('span');
        label.className = 'report-print-cards__field-label';
        // textContent only — card values are data, never markup.
        label.textContent = `${field.header}: `;
        const value = document.createElement('span');
        value.textContent = field.render(row);
        line.appendChild(label);
        line.appendChild(value);
        card.appendChild(line);
      }

      if (config.signatureLabel) {
        const signature = document.createElement('div');
        signature.className = 'report-print-cards__signature';
        const ruledLine = document.createElement('div');
        ruledLine.className = 'report-print-cards__signature-line';
        const label = document.createElement('span');
        label.textContent = config.signatureLabel;
        signature.appendChild(ruledLine);
        signature.appendChild(label);
        card.appendChild(signature);
      }

      grid.appendChild(card);
    }
    sheet.appendChild(grid);

    return sheet;
  }

  /** Injects the print stylesheet once per document load. */
  private ensureStyles(): void {
    if (document.getElementById(ReportPdfService.STYLE_ID)) {
      return;
    }

    const style = document.createElement('style');
    style.id = ReportPdfService.STYLE_ID;
    style.textContent = this.printStyles();
    document.head.appendChild(style);
  }

  /**
   * The print stylesheet — one source of truth: injected by `ensureStyles` for the in-page
   * pipeline, inlined by `standaloneDocument` for the previewed document.
   */
  private printStyles(): string {
    return `
      .${ReportPdfService.SHEET_CLASS} {
        display: none;
        direction: rtl;
        padding: 16px;
        color: #000;
        background: #fff;
        font-family: inherit;
      }
      .${ReportPdfService.SHEET_CLASS}__title {
        font-size: 18px;
        font-weight: 700;
        margin: 0 0 4px;
      }
      .${ReportPdfService.SHEET_CLASS}__subtitle {
        font-size: 13px;
        margin: 0 0 8px;
      }
      .${ReportPdfService.SHEET_CLASS}__meta {
        display: flex;
        flex-wrap: wrap;
        gap: 4px 24px;
        font-size: 13px;
        margin: 0 0 12px;
      }
      .${ReportPdfService.SHEET_CLASS} table {
        width: 100%;
        border-collapse: collapse;
      }
      .${ReportPdfService.SHEET_CLASS} th,
      .${ReportPdfService.SHEET_CLASS} td {
        border: 1px solid #333;
        padding: 6px 8px;
        font-size: 12px;
        text-align: right;
      }
      .${ReportPdfService.FORM_CLASS} {
        display: none;
        direction: rtl;
        padding: 16px;
        color: #000;
        background: #fff;
        font-family: inherit;
      }
      .${ReportPdfService.FORM_CLASS}__title {
        font-size: 18px;
        font-weight: 700;
        margin: 0 0 4px;
        text-align: center;
      }
      .${ReportPdfService.FORM_CLASS}__subtitle {
        font-size: 13px;
        margin: 0 0 12px;
        text-align: center;
      }
      .${ReportPdfService.FORM_CLASS}__section {
        margin: 0 0 12px;
        break-inside: avoid;
      }
      .${ReportPdfService.FORM_CLASS}__section-title {
        font-size: 14px;
        font-weight: 700;
        border-bottom: 2px solid #333;
        padding-bottom: 2px;
        margin: 0 0 4px;
      }
      .${ReportPdfService.FORM_CLASS}__field {
        display: flex;
        gap: 6px;
        padding: 8px 0 2px;
        font-size: 13px;
      }
      .${ReportPdfService.FORM_CLASS}__field-label {
        white-space: nowrap;
        font-weight: 600;
      }
      .${ReportPdfService.FORM_CLASS}__field-line {
        flex: 1;
        border-bottom: 1px dotted #444;
        min-height: 16px;
      }
      .${ReportPdfService.FORM_CLASS} table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 4px;
      }
      .${ReportPdfService.FORM_CLASS} th,
      .${ReportPdfService.FORM_CLASS} td {
        border: 1px solid #333;
        padding: 6px 8px;
        font-size: 12px;
        text-align: right;
        height: 24px;
      }
      .${ReportPdfService.FORM_CLASS}__note-line {
        border-bottom: 1px dotted #444;
        height: 26px;
        margin-top: 10px;
      }
      .${ReportPdfService.FORM_CLASS}__signatures {
        display: flex;
        justify-content: space-between;
        gap: 24px;
        margin-top: 24px;
      }
      .${ReportPdfService.FORM_CLASS}__signature {
        flex: 1;
        text-align: center;
        font-size: 13px;
      }
      .${ReportPdfService.FORM_CLASS}__signature-line {
        border-bottom: 1px solid #333;
        height: 34px;
        margin-bottom: 4px;
      }
      .${ReportPdfService.FORM_CLASS}__footer {
        margin-top: 16px;
        font-size: 11px;
        text-align: center;
      }
      .${ReportPdfService.CARDS_CLASS} {
        display: none;
        direction: rtl;
        padding: 16px;
        color: #000;
        background: #fff;
        font-family: inherit;
      }
      .${ReportPdfService.CARDS_CLASS}__title {
        font-size: 18px;
        font-weight: 700;
        margin: 0 0 4px;
      }
      .${ReportPdfService.CARDS_CLASS}__subtitle {
        font-size: 13px;
        margin: 0 0 8px;
      }
      .${ReportPdfService.CARDS_CLASS}__meta {
        display: flex;
        flex-wrap: wrap;
        gap: 4px 24px;
        font-size: 13px;
        margin: 0 0 12px;
      }
      .${ReportPdfService.CARDS_CLASS}__grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 6mm 5mm;
      }
      .${ReportPdfService.CARDS_CLASS}__card {
        border: 1px dashed #444;
        border-radius: 4px;
        padding: 5mm 4mm;
        break-inside: avoid;
      }
      .${ReportPdfService.CARDS_CLASS}__field {
        display: flex;
        gap: 6px;
        padding: 3px 0;
        font-size: 12px;
      }
      .${ReportPdfService.CARDS_CLASS}__field-label {
        white-space: nowrap;
        font-weight: 600;
      }
      .${ReportPdfService.CARDS_CLASS}__signature {
        margin-top: 12px;
        font-size: 12px;
      }
      .${ReportPdfService.CARDS_CLASS}__signature-line {
        border-bottom: 1px solid #333;
        height: 30px;
        margin-bottom: 4px;
      }
      @page {
        size: A4 portrait;
        margin: 12mm;
      }
      @media print {
        body > *:not(.${ReportPdfService.SHEET_CLASS}):not(.${ReportPdfService.FORM_CLASS}):not(.${ReportPdfService.CARDS_CLASS}) {
          display: none !important;
        }
        .${ReportPdfService.SHEET_CLASS} {
          display: block !important;
        }
        .${ReportPdfService.FORM_CLASS} {
          display: block !important;
        }
        .${ReportPdfService.CARDS_CLASS} {
          display: block !important;
        }
      }
    `;
  }

  /**
   * Wraps a print element into a complete standalone RTL document (18-40 preview). The SAME
   * print styles are inlined; a visibility override un-hides the root class — the in-page
   * pipeline hides it outside print media, the framed document shows it on screen. Arabic
   * shaping comes from the host browser's engine (the epic-wide ruling), not a font asset.
   */
  private standaloneDocument(element: HTMLElement, documentTitle: string): ReportPreviewSource {
    const overrides = [ReportPdfService.SHEET_CLASS, ReportPdfService.FORM_CLASS, ReportPdfService.CARDS_CLASS]
      .map(c => `.${c} { display: block; }`)
      .join('\n');
    const title = this.escapeHtml(documentTitle || 'report');
    return {
      documentTitle: documentTitle || 'report',
      html: [
        '<!DOCTYPE html>',
        '<html dir="rtl" lang="ar">',
        '<head>',
        '<meta charset="utf-8">',
        `<title>${title}</title>`,
        `<style>${this.printStyles()}</style>`,
        `<style>${overrides}</style>`,
        '</head>',
        `<body>${element.outerHTML}</body>`,
        '</html>'
      ].join('\n')
    };
  }

  /** Only the <title> is interpolated as markup — every data cell goes through textContent. */
  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
