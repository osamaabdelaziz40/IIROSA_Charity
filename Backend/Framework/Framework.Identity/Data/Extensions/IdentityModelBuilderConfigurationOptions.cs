using Framework.Core.Data;

namespace Framework.Identity.Data.Extensions
{
    public class IdentityModelBuilderConfigurationOptions : ModelBuilderConfigurationOptions
    {
        public const string DefaultDbSchema = "identity";

        public IdentityModelBuilderConfigurationOptions(
             string schema = DefaultDbSchema)
            : base(schema)
        {
        }
    }
}