/**
 * UC-RPT-26 — the blank field-survey questionnaire (استبانة), both variants.
 *
 * The questionnaire is a STATIC document: blank lines a field officer completes by
 * hand when surveying a household — no stored data, no endpoint (§23.U.26; the
 * legacy MVC print controller is superseded by the epic-wide browser-print ruling,
 * 18-21). The definition is data-driven so 18-40's generic viewer can reuse it;
 * every label is an i18n key relative to `reports.surveyQuestionnaire`
 * (sections.* / fields.* / signatures.*) — never a hard-coded string.
 *
 * Section/field lists mirror the family/orphan registration fields the platform
 * already models (§13/§11); lines the platform has no equivalent for are
 * form-only — nothing here writes to the DB.
 */

export type QuestionnaireVariant = 'family' | 'widow';

/** A labelled blank line (form-only field). */
export interface QuestionnaireField {
  labelKey: string;
}

/** A repeated blank-row grid (بيانات الأيتام / بيانات الأبناء). */
export interface QuestionnaireGrid {
  columnKeys: string[];
  rowCount: number;
}

export interface QuestionnaireSection {
  titleKey: string;
  fields?: QuestionnaireField[];
  grid?: QuestionnaireGrid;
  /** Ruled free lines for the notes block. */
  noteLines?: number;
  /** Signature slot labels under the notes block. */
  signatureKeys?: string[];
}

export interface QuestionnaireDocument {
  variant: QuestionnaireVariant;
  /** i18n key of the printed document title (family.title / widow.title). */
  titleKey: string;
  sections: QuestionnaireSection[];
}

/** بيانات الأيتام — the orphan grid the family variant carries (§13/§11 field set). */
const ORPHANS_GRID: QuestionnaireGrid = {
  columnKeys: ['name', 'birthDate', 'gender', 'educationStage', 'healthStatus'],
  rowCount: 6
};

/** بيانات الأبناء — the widow variant's children grid (same skeleton, no health column). */
const CHILDREN_GRID: QuestionnaireGrid = {
  columnKeys: ['name', 'birthDate', 'gender', 'educationStage'],
  rowCount: 6
};

/** السكن والدخل + ملاحظات وتوقيعات — shared by both variants. */
const SHARED_TAIL: QuestionnaireSection[] = [
  {
    titleKey: 'housingIncome',
    fields: [
      { labelKey: 'ownership' },
      { labelKey: 'rent' },
      { labelKey: 'housingType' },
      { labelKey: 'incomeValue' }
    ]
  },
  {
    titleKey: 'notes',
    noteLines: 3,
    signatureKeys: ['officer', 'manager']
  }
];

/** استبانة الأسرة (default) — family / provider / orphans / mother + shared tail. */
function buildFamilyQuestionnaire(): QuestionnaireDocument {
  return {
    variant: 'family',
    titleKey: 'family.title',
    sections: [
      {
        titleKey: 'familyData',
        fields: [
          { labelKey: 'governorate' },
          { labelKey: 'center' },
          { labelKey: 'village' },
          { labelKey: 'address' },
          { labelKey: 'mobile' }
        ]
      },
      {
        titleKey: 'providerData',
        fields: [
          { labelKey: 'name' },
          { labelKey: 'nationalId' },
          { labelKey: 'relation' },
          { labelKey: 'qualification' },
          { labelKey: 'job' },
          { labelKey: 'healthStatus' },
          { labelKey: 'socialStatus' }
        ]
      },
      { titleKey: 'orphansData', grid: ORPHANS_GRID },
      {
        titleKey: 'motherData',
        fields: [
          { labelKey: 'name' },
          { labelKey: 'nationalId' },
          { labelKey: 'birthDate' },
          { labelKey: 'job' },
          { labelKey: 'healthStatus' }
        ]
      },
      ...SHARED_TAIL
    ]
  };
}

/** نسخة الأرامل — widow / children + shared tail (the widow question set swapped in). */
function buildWidowQuestionnaire(): QuestionnaireDocument {
  return {
    variant: 'widow',
    titleKey: 'widow.title',
    sections: [
      {
        titleKey: 'widowData',
        fields: [
          { labelKey: 'name' },
          { labelKey: 'nationalId' },
          { labelKey: 'husbandDeathDate' },
          { labelKey: 'qualification' },
          { labelKey: 'job' },
          { labelKey: 'healthStatus' }
        ]
      },
      { titleKey: 'childrenData', grid: CHILDREN_GRID },
      ...SHARED_TAIL
    ]
  };
}

export function buildSurveyQuestionnaire(variant: QuestionnaireVariant): QuestionnaireDocument {
  return variant === 'widow' ? buildWidowQuestionnaire() : buildFamilyQuestionnaire();
}
