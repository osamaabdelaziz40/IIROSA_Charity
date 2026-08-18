using JetBrains.Annotations;

namespace Framework.Core.Data
{
    public class ModelBuilderConfigurationOptions
    {
        [CanBeNull]
        public string Schema { get; set; }

        public ModelBuilderConfigurationOptions(
            [CanBeNull] string schema = null)
        {
            Schema = schema;
        }
    }
}