// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemSetting.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using Framework.Core.Extensions;

namespace Framework.Core.SharedServices.Entities
{
    public class SystemSetting : FullAuditedEntityBase<int>
    {
        public string Name { get; private set; }
        public string ValueType { get; private set; }
        public string Value { get; private set; }
        public string GroupName { get; private set; }
        public bool IsSecure { get; private set; }
        public bool IsSticky { get; private set; }
        public bool IsActive { get; private set; }
        private SystemSetting()
        {

        }
        public SystemSetting(int id, string name, string valueType, string value, string groupName, bool isSecure, bool isSticky, bool isActive)
        {
            Id = id.AgainstNegativeOrZero(nameof(id));
            Name = name.AgainstNullOrEmpty(nameof(name));
            ValueType = valueType.AgainstNullOrEmpty(nameof(valueType));
            Value = value.AgainstNull(nameof(value));
            GroupName = groupName;
            IsSecure = isSecure;
            IsSticky = isSticky;
            IsActive = isActive;
        }
        public void UpdateValue(string value)
        { 
            Value = value;
        }
    }
}