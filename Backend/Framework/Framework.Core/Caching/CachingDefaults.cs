namespace Framework.Core.Caching
{
    /// <summary>
    /// Represents default values related to caching
    /// </summary>
    public static partial class CachingDefaults
    {
        /// <summary>
        /// Gets the default cache time in minutes
        /// </summary>
        public static int CacheTime => 60;

        public static int LookupsCacheTime => 0; // MINUTES=> ZERO MEANS IT WILL NOT CACHING

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static string SettingsAllCacheKey => "settings.all";

        public static string LookupWeightMethods => "LookupWeightMethods";
        public static string LookupRequestStatus => "LookupRequestStatus";
        public static string LookupChequeBeneficiary => "LookupChequeBeneficiary";
        public static string LookupNgoTypes => "LookupNgoTypes";
        public static string LookupOutgoingCategory => "LookupOutgoingCategory";
        public static string LookupMissionType => "LookupMissionType";
        public static string LookupMissionTimeType     => "LookupMissionTimeType";
        public static string LookupDevelopmentProject  => "LookupDevelopmentProject";
        public static string LookupOfficeProjectType   => "LookupOfficeProjectType";

        public static string LookupDepartment => "LookupDepartment";
        public static string LookupCountry => "LookupCountry";
        public static string LookupCenter => "LookupCenter";
        public static string LookupRelation => "LookupRelation";
        public static string LookupRegion => "LookupRegion";
        public static string LookupCurrency => "LookupCurrency";
        public static string LookupBank => "LookupBank";
        public static string LookupLaborCardType => "LookupLaborCardType";
        public static string LookupCompanyCategory => "LookupCompanyCategory";
        public static string LookupLicenseType => "LookupLicenseType";
        public static string LookupLegalType => "LookupLegalType";
        public static string LookupEmployeeType => "LookupEmployeeType";
        public static string LookupRequestTypes => "LookupRequestTypes";
        public static string LookupIndexTypes => "LookupIndexTypes";
        public static string LookupQuestionTypes => "LookupQuestionTypes";
        public static string LookupAnswerTypes => "LookupAnswerTypes";
        public static string LookupsAllCacheKey => "lookups";
        public static string LookupsUserTypes=> "LookupsUserTypes";
        public static string LookupsRoles => "LookupsRoles";
        public static string LookupsLevels => "LookupsLevels";
        public static string SurveyTakerAgeRange => "SurveyTakerAgeRange";
        public static string SurveyTakerArea => "SurveyTakerArea";
        public static string SurveyTakerDeviceType => "SurveyTakerDeviceType";
        public static string SurveyTakerEducationLevel => "SurveyTakerEducationLevel";
        public static string SurveyTakerGender => "SurveyTakerGender";
    }
}