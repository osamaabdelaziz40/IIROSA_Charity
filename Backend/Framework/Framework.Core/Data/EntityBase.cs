using Framework.Core.Data.Events;
using Framework.Core.Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Core.Data
{
    [Serializable]
    public abstract class EntityBase : IEntityBase
    {
        public List<BaseDomainEvent> Events = new List<BaseDomainEvent>();
    }

    [Serializable]
    public abstract class EntityBase<TKey> : EntityBase, IEntityBase<TKey>
    {
        public TKey Id { get; set; }
    }

    [Serializable]
    public abstract class FullAuditedEntityBase : EntityBase
    {
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }

    [Serializable]
    public abstract class FullAuditedEntityBase<TKey> : EntityBase<TKey>
    {
        //[Column(Order = 300)]
        public string CreatedBy { get; set; }

        //[Column(Order = 301)]
        public DateTime CreatedOn { get; set; }

        //[Column(Order = 302)]
        public string UpdatedBy { get; set; }

        //[Column(Order = 303)]
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }

    [Serializable]
    public class LookupEntityBase : FullAuditedEntityBase
    {
        public int Id { get; set; }
        public string NameAr { get; set; }

        public string NameEn { get; set; }
        public string Name { get { return CultureHelper.IsArabic ? this.NameAr : this.NameEn; } }

        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    [Serializable]
    public class LookupEntityBase<TKey> : FullAuditedEntityBase<TKey>
    {
        public string NameAr { get; set; }

        public string? NameEn { get; set; }

        [NotMapped]
        public string Name { get { return CultureHelper.IsArabic ? this.NameAr : this.NameEn; } }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
    public class LookupBaseDto<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
}