using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Bank Details DTO - Used for setting bank account details (UC-3.9)
/// </summary>
public class CharityBankDetailsDto
{
    [Required]
    public Guid CharityId { get; set; }

    public int? BankId { get; set; }

    [StringLength(50, ErrorMessage = "Bank account cannot exceed 50 characters")]
    public string? BankAccount { get; set; }

    [StringLength(34, ErrorMessage = "IBAN cannot exceed 34 characters")]
    public string? IBAN { get; set; }
}
