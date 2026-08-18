namespace IIROSA.Domain.Configurations;

/// <summary>
/// Contains default mapping constants for entity configurations
/// </summary>
public static class MappingDefaults
{
    /// <summary>
    /// Schema name for lookup entities (all entities inheriting from LookupEntity)
    /// </summary>
    public const string LOOKUP_SCHEMA = "Lookup";

    /// <summary>
    /// Schema name for main business entities (all entities inheriting from FullAuditedEntity)
    /// </summary>
    public const string IIROSA_SCHEMA = "IIROSA";
}
