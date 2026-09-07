namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// OrphanPayment List DTO - Used in grid/list views (UC-5.8)
/// </summary>
public class OrphanPaymentListDto
{
    public Guid Id { get; set; }
    public string? BatchNo { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime PaymentPeriodFrom { get; set; }
    public DateTime PaymentPeriodTo { get; set; }
    public int OrphanCount { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? Currency { get; set; }
    public bool IsBatchUploaded { get; set; }
    public DateTime GroupDate { get; set; }

    // Distribution start date تاريخ بدء التوزيع — §15.S.1 التاريخ column; null on legacy rows
    public DateTime? PaymentDate { get; set; }
    public string? CreatedByName { get; set; }
}
