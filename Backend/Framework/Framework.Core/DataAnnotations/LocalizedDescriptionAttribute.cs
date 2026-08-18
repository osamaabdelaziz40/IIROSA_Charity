using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Framework.Core.DataAnnotations
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;
        private readonly ResourceManager _resource;
        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resource = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string displayName = _resource.GetString(_resourceKey);

                return string.IsNullOrEmpty(displayName)
                    ? $"[[{_resourceKey}]]"
                    : displayName;
            }
        }

        public string GetDescriptionByCertainCulture(string culture = "ar")
        {
            string displayName = _resource.GetString(_resourceKey, new CultureInfo(culture));

            return string.IsNullOrEmpty(displayName)
                ? $"[[{_resourceKey}]]"
                : displayName;
        }
    }


}
