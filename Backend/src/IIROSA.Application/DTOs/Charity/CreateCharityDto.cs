using System.ComponentModel.DataAnnotations;
using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Create Charity DTO - Used for creating new charity (UC-3.1)
/// </summary>
public class CreateCharityDto
{
    // Basic Information
    [Required(ErrorMessage = "Charity name is required")]
    [StringLength(200, ErrorMessage = "Charity name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "NGO Type is required")]
    [StringLength(100, ErrorMessage = "NGO Type cannot exceed 100 characters")]
    public string NGOType { get; set; } = string.Empty;

    // Contact Information
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street name is required")]
    [StringLength(200, ErrorMessage = "Street name cannot exceed 200 characters")]
    public string StreetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Village is required")]
    [StringLength(100, ErrorMessage = "Village cannot exceed 100 characters")]
    public string Village { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mail box is required")]
    [StringLength(20, ErrorMessage = "Mail box cannot exceed 20 characters")]
    public string MailBox { get; set; } = string.Empty;

    // Phone/Contact
    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone2 is required")]
    [StringLength(20, ErrorMessage = "Phone2 cannot exceed 20 characters")]
    public string Phone2 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Home phone is required")]
    [StringLength(20, ErrorMessage = "Home phone cannot exceed 20 characters")]
    public string HomePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fax is required")]
    [StringLength(20, ErrorMessage = "Fax cannot exceed 20 characters")]
    public string Fax { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    // Location
    [Required(ErrorMessage = "Country is required")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Region is required")]
    public int RegionId { get; set; }

    [Required(ErrorMessage = "Center is required")]
    public int CenterId { get; set; }

    [Required(ErrorMessage = "Map location is required")]
    [StringLength(500, ErrorMessage = "Map location cannot exceed 500 characters")]
    public string NgoMapLocation { get; set; } = string.Empty;

    // Banking
    [Required(ErrorMessage = "Bank is required")]
    public int BankId { get; set; }

    [Required(ErrorMessage = "Bank account is required")]
    [StringLength(50, ErrorMessage = "Bank account cannot exceed 50 characters")]
    public string BankAccount { get; set; } = string.Empty;

    [Required(ErrorMessage = "IBAN is required")]
    [StringLength(34, ErrorMessage = "IBAN cannot exceed 34 characters")]
    public string IBAN { get; set; } = string.Empty;

    // Management - Boss
    [Required(ErrorMessage = "Boss name is required")]
    [StringLength(100, ErrorMessage = "Boss name cannot exceed 100 characters")]
    public string BossName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boss job name is required")]
    [StringLength(100, ErrorMessage = "Boss job name cannot exceed 100 characters")]
    public string BossJobName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boss phone1 is required")]
    [StringLength(20, ErrorMessage = "Boss phone1 cannot exceed 20 characters")]
    public string BossPhone1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boss phone2 is required")]
    [StringLength(20, ErrorMessage = "Boss phone2 cannot exceed 20 characters")]
    public string BossPhone2 { get; set; } = string.Empty;

    // Management - Responsible
    [Required(ErrorMessage = "Responsible job name is required")]
    [StringLength(100, ErrorMessage = "Responsible job name cannot exceed 100 characters")]
    public string ResponsibleJobName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responsible phone1 is required")]
    [StringLength(20, ErrorMessage = "Responsible phone1 cannot exceed 20 characters")]
    public string ResponsiblePhone1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responsible phone2 is required")]
    [StringLength(20, ErrorMessage = "Responsible phone2 cannot exceed 20 characters")]
    public string ResponsiblePhone2 { get; set; } = string.Empty;

    // Settings
    public List<AttachmentDto>? Icon_Attach { get; set; }

    public bool ReceivingDonations { get; set; } = true;

    [Required(ErrorMessage = "Notes are required")]
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string Notes { get; set; } = string.Empty;

    // User Account
    public bool CreateUserAccount { get; set; } = true;

    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format for username")]
    public string? Username { get; set; }

    [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }
}
