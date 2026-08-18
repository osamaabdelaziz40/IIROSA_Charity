using Framework.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Framework.Core.Data
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, string userName , string token="")
        {
            changeTracker.DetectChanges();

            var timestamp = DateTime.Now;

            userName = userName ?? "Anonymous";
            const string CreatedOnProperty = "CreatedOn";
            const string CreatedByProperty = "CreatedBy";

            foreach (var entry in changeTracker.Entries())
            {
                if (entry.Properties.Any(a => a.Metadata.Name == "CreatedOn"))
                {
                    if (entry.State == EntityState.Added && entry.Entity is IEntityBase)
                    {
                        if (entry.Property("CreatedBy").CurrentValue == null)
                        {
                            entry.Property("CreatedBy").CurrentValue = userName;
                        }
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                    }

                    if (entry.State == EntityState.Modified && entry.Entity is IEntityBase)
                    {
                        if (entry.Property("UpdatedBy").CurrentValue == null)
                        {
                            entry.Property("UpdatedBy").CurrentValue = userName;
                        }
                        entry.Property("UpdatedOn").CurrentValue = timestamp;

                        // Yousra: Mark CreatedByProperty as not modified in update to preserve the
                        // original value inserted in DB especially in case you donot submit the whole
                        // model from view and send specific data
                        entry.Property(CreatedByProperty).IsModified = false;
                        entry.Property(CreatedOnProperty).IsModified = false;
                    }
                }
                //Set Guid Id Sequential
                if (entry.Entity.GetType().GetProperty("Id") != null)
                {
                    Guid id;
                    if (Guid.TryParse(entry.Property("Id").CurrentValue.ToString(), out id) && id == Guid.Empty)
                    {
                        entry.Property("Id").CurrentValue = Guid.NewGuid().AsSequentialGuid();
                    }
                }
            }
        }

        public static void Validate(this ChangeTracker changeTracker)
        {
            var validationResults = new List<ValidationResult>();
            foreach (var entry in changeTracker.Entries())
            {
                string validationErrors = null;
                if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entry.Entity, new ValidationContext(entry.Entity), validationResults))
                {
                    foreach (var item in validationResults)
                    {
                        validationErrors +=
                            $"Entity: {entry.Entity} - Property: {item.MemberNames} - Error: {item.ErrorMessage} \n ";
                    }
                    throw new ValidationException(validationErrors);
                }
            }
        }
    }
}