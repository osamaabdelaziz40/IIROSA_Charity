using Framework.Core;

namespace Framework.Identity.Data.Dtos
{
    public class ImportedAggregatorDto
    {
        [SourceNames("ROWNUMBER")]
        public int ROWNUMBER { get; set; }

        [SourceNames("TID")]
        public string TID { get; set; }

        [SourceNames("TSM")]
        public string TSM { get; set; }

        [SourceNames("ModelName")]
        public string ModelName { get; set; }

        [SourceNames("BankName")]
        public string BankName { get; set; }

        [SourceNames("MID")]
        public string MID { get; set; }

        [SourceNames("FullTID")]
        public string FullTID { get; set; }

        [SourceNames("SiteID")]
        public string SiteID { get; set; }

        [SourceNames("MerchantNameEn")]
        public string MerchantNameEn { get; set; }

        [SourceNames("MerchantNameAr")]
        public string MerchantNameAr { get; set; }

        [SourceNames("OutletNameEn")]
        public string OutletNameEn { get; set; }

        [SourceNames("OutletNameAr")]
        public string OutletNameAr { get; set; }

        [SourceNames("AddressEn")]
        public string AddressEn { get; set; }

        [SourceNames("AddressAr")]
        public string AddressAr { get; set; }

        [SourceNames("City")]
        public string City { get; set; }

        [SourceNames("Telephone")]
        public string Telephone { get; set; }

        [SourceNames("Mobile")]
        public string Mobile { get; set; }

        [SourceNames("MerchantCategoryCode")]
        public string MerchantCategoryCode { get; set; }

        [SourceNames("MerchantGroup")]
        public string MerchantGroup { get; set; }

        [SourceNames("CRN")]
        public string CRN { get; set; }

        [SourceNames("HWSerialNumber")]
        public string HWSerialNumber { get; set; }

        [SourceNames("ConnectioMethod")]
        public string ConnectioMethod { get; set; }

        [SourceNames("ConnectionProvider")]
        public string ConnectionProvider { get; set; }

        [SourceNames("NSPProvider")]
        public string NSPProvider { get; set; }

        [SourceNames("Email")]
        public string Email { get; set; }

        [SourceNames("NationalId")]
        public string NationalId { get; set; }

        [SourceNames("LicenceType")]
        public string LicenceType { get; set; }

        [SourceNames("AverageValue")]
        public string AverageValue { get; set; }

        [SourceNames("BuldingNumber")]
        public string BuldingNumber { get; set; }

        [SourceNames("StreetName")]
        public string StreetName { get; set; }

        [SourceNames("Neighborhood")]
        public string Neighborhood { get; set; }

        [SourceNames("AccountNumber")]
        public string AccountNumber { get; set; }

        [SourceNames("JobTitle")]
        public string JobTitle { get; set; }

        [SourceNames("Entity")]
        public string Entity { get; set; }

        [SourceNames("BenficieryFullName")]
        public string BenficieryFullName { get; set; }

        [SourceNames("BenficieryBank")]
        public string BenficieryBank { get; set; }

        [SourceNames("BenficieryStreetName")]
        public string BenficieryStreetName { get; set; }

        [SourceNames("BenficieryCountry")]
        public string BenficieryCountry { get; set; }

        [SourceNames("BenficieryCity")]
        public string BenficieryCity { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}