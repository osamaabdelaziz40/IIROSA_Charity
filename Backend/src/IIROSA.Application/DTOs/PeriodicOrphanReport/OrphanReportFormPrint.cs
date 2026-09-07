using IIROSA.Domain.Entities;

// The entity type is fully qualified below — inside this namespace the bare name
// 'PeriodicOrphanReport' resolves to the namespace itself (CS0118).
using ReportEntity = global::IIROSA.Domain.Entities.PeriodicOrphanReport;

namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — the print payload served by
/// <c>POST /api/Reports/orphan-report-form/export/pdf</c>. Per the recorded client-side
/// print ruling (epics 9/10/18) the endpoint returns composed data, not PDF bytes: the SPA
/// renders the official form and the browser's print-to-PDF produces the file.
/// </summary>
public class OrphanReportFormPrintDto
{
    /// <summary>
    /// The resolved layout variant — the legacy 24-template matrix collapsed to a stable key:
    /// base (<c>disabled</c> &gt; <c>studying</c> &gt; <c>not-studying</c>) plus the document
    /// sections present (<c>+marriage</c>, <c>+death</c>, <c>+medical</c>, <c>+cert</c>),
    /// e.g. <c>studying+cert+medical</c>. Drives the client's section/image layout.
    /// </summary>
    public string Variant { get; set; } = "not-studying";

    /// <summary>The full 9-4 detail data (caller-scoped read — the same shape as GET by id).</summary>
    public PeriodicOrphanReportDto Report { get; set; } = new();

    /// <summary>The attachment slots present on this report (BR-12 — ids only, never bytes).</summary>
    public List<OrphanReportFormAttachmentSlotDto> Attachments { get; set; } = new();
}

/// <summary>One document slot carried into the print payload; the SPA fetches the bytes by id.</summary>
public class OrphanReportFormAttachmentSlotDto
{
    /// <summary>Stable slot key: orphanPhoto · certificate · medicalReport · deathCertificate · marriageContract.</summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>The attachment id (the single truth — BR-12).</summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Pure variant resolver for UC-ORR-17 (§14.1 printing rule) — no I/O, unit-testable.
/// Base layout: a disabled orphan overrides studying; otherwise education flags decide;
/// otherwise not-studying. Document sections join the key when their image id is present.
/// </summary>
public static class OrphanReportFormVariantResolver
{
    public static string Resolve(ReportEntity report)
    {
        var baseKey = !string.IsNullOrWhiteSpace(report.Disability)
            ? "disabled"
            : IsStudying(report) ? "studying" : "not-studying";

        var sections = new List<string>(4);
        if (report.OrphanMarriageImageId.HasValue) sections.Add("marriage");
        if (report.OrphanDeadImageId.HasValue) sections.Add("death");
        if (report.MedicalReportImageId.HasValue) sections.Add("medical");
        if (report.OrphanCertificateImageId.HasValue) sections.Add("cert");

        return sections.Count == 0 ? baseKey : $"{baseKey}+{string.Join("+", sections)}";
    }

    /// <summary>Studying = the orphan-student flag, or any live education anchor (level/school).</summary>
    private static bool IsStudying(ReportEntity report) =>
        report.IsOrphanStudent == true
        || report.EducationalLevelId.HasValue
        || !string.IsNullOrWhiteSpace(report.School);
}
