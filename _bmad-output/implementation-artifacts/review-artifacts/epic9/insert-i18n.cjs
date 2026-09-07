// Review P26 2026-08-24: adds the keys the history/compare/schedule screens reference
// but that exist in NEITHER locale. Idempotent: skips keys already present.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '../../../..');
const io = (f) => path.join(root, 'Frontend/src/assets/i18n', f);

const orphanReportsAr = {
  history: 'سجل التقارير',
  schedule: 'جدولة التقارير',
  compare: 'مقارنة التقارير',
  untitledReport: 'تقرير دوري بدون رقم',
  dateFrom: 'من تاريخ',
  dateTo: 'إلى تاريخ',
  female: 'الاناث',
  male: 'الذكور',
  pending: 'قيد الانتظار',
  active: 'نشط',
  inactive: 'متوقف',
  toggleActive: 'تفعيل / إيقاف',
  all: 'الكل',
  years: 'سنوات',
  change: 'التغير',
  percentChange: 'نسبة التغير',
  selectReport: 'اختر تقريراً',
  report1: 'التقرير الأول',
  report2: 'التقرير الثاني',
  totalOrphans: 'إجمالي الأيتام',
  sponsored: 'مكفول',
  unsponsored: 'غير مكفول',
  gender: 'النوع',
  ageDistribution: 'التوزيع العمري',
  genderDistribution: 'التوزيع حسب النوع',
  charityDistribution: 'التوزيع حسب الجمعيات',
  comparisonMetrics: 'مقاييس المقارنة',
  ageFrom: 'العمر من',
  ageTo: 'العمر إلى',
  day: 'يوم',
  dayOfMonth: 'يوم من الشهر',
  dayOfMonthHint: 'يوم تشغيل التقرير الدوري شهرياً',
  frequency: 'التكرار',
  reportName: 'اسم التقرير',
  reportNamePlaceholder: 'اسم مميز لهذا التقرير',
  createdOn: 'تاريخ الإنشاء',
  emailRecipients: 'المستلمون بالبريد',
  emailRecipientsPlaceholder: 'بريد إلكتروني مفصول بفواصل',
  emailRecipientsHint: 'يُرسل التقرير المجدول إلى هذه العناوين عند موعده',
  includeEducationDetails: 'تضمين التفاصيل التعليمية',
  includeFamilyDetails: 'تضمين تفاصيل الأسرة',
  includeHealthDetails: 'تضمين التفاصيل الصحية',
  sponsorshipStatus: 'حالة الكفالة',
  noHistory: 'لا يوجد سجل',
  noHistoryMessage: 'لم يتم تشغيل أي تقرير بعد',
  noScheduledReports: 'لا توجد تقارير مجدولة',
  noScheduledReportsMessage: 'لم يتم جدولة أي تقرير دوري بعد',
  breadcrumb: {
    history: 'سجل التقارير',
    compare: 'مقارنة التقارير',
    schedule: 'جدولة التقارير'
  }
};

const orphanReportsEn = {
  history: 'Report History',
  schedule: 'Schedule Reports',
  compare: 'Compare Reports',
  untitledReport: 'Untitled periodic report',
  dateFrom: 'From date',
  dateTo: 'To date',
  female: 'Female',
  male: 'Male',
  pending: 'Pending',
  active: 'Active',
  inactive: 'Inactive',
  toggleActive: 'Activate / Deactivate',
  all: 'All',
  years: 'Years',
  change: 'Change',
  percentChange: '% change',
  selectReport: 'Select a report',
  report1: 'Report 1',
  report2: 'Report 2',
  totalOrphans: 'Total orphans',
  sponsored: 'Sponsored',
  unsponsored: 'Unsponsored',
  gender: 'Gender',
  ageDistribution: 'Age distribution',
  genderDistribution: 'Gender distribution',
  charityDistribution: 'Charity distribution',
  comparisonMetrics: 'Comparison metrics',
  ageFrom: 'Age from',
  ageTo: 'Age to',
  day: 'Day',
  dayOfMonth: 'Day of month',
  dayOfMonthHint: 'The day of the month the recurring report runs',
  frequency: 'Frequency',
  reportName: 'Report name',
  reportNamePlaceholder: 'A distinct name for this report',
  createdOn: 'Created on',
  emailRecipients: 'Email recipients',
  emailRecipientsPlaceholder: 'Comma-separated email addresses',
  emailRecipientsHint: 'The scheduled report is emailed to these addresses when due',
  includeEducationDetails: 'Include education details',
  includeFamilyDetails: 'Include family details',
  includeHealthDetails: 'Include health details',
  sponsorshipStatus: 'Sponsorship status',
  noHistory: 'No history',
  noHistoryMessage: 'No report has been run yet',
  noScheduledReports: 'No scheduled reports',
  noScheduledReportsMessage: 'No recurring report has been scheduled yet',
  breadcrumb: {
    history: 'Report History',
    compare: 'Compare Reports',
    schedule: 'Schedule Reports'
  }
};

const commonAr = { exportExcel: 'استخراج Excel' };
const commonEn = { exportExcel: 'Export to Excel' };

function apply(file, additions) {
  const p = io(file);
  const obj = JSON.parse(fs.readFileSync(p, 'utf8'));
  let added = 0, skipped = 0;
  for (const [ns, kv] of Object.entries(additions)) {
    for (const [k, v] of Object.entries(kv)) {
      if (!(k in obj[ns])) { obj[ns][k] = v; added++; }
      else skipped++;
    }
  }
  fs.writeFileSync(p, JSON.stringify(obj, null, 2) + '\n');
  console.log(`${file}: added ${added}, skipped ${skipped}`);
}

apply('ar.json', { orphanReports: orphanReportsAr, common: commonAr });
apply('en.json', { orphanReports: orphanReportsEn, common: commonEn });
