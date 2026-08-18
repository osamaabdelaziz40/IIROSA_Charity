namespace Framework.Core.Data
{
    public static class ModelBuilderExtensions
    {
        /*

        #region Shadow Properties

        public static void AddShadowProperties(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var type = entityType.ClrType;

                // set auditing properties
                if (typeof(IShadowProperties).IsAssignableFrom(type))
                {
                    SetShadowPropertiesMethodInfo
                        .MakeGenericMethod(type)
                        .Invoke(modelBuilder, new object[] { modelBuilder });
                }

                // set soft delete property
                if (typeof(ISoftDelete).IsAssignableFrom(type))
                {
                    var method = SetIsDeletedShadowPropertyMethodInfo.MakeGenericMethod(type);
                    method.Invoke(modelBuilder, new object[] { modelBuilder });
                }
            }
        }

        private static readonly MethodInfo SetIsDeletedShadowPropertyMethodInfo
            = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(SetIsDeletedShadowProperty),
                BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo SetShadowPropertiesMethodInfo
            = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(SetShadowProperties),
                BindingFlags.Public | BindingFlags.Static);

        public static void SetIsDeletedShadowProperty<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDelete
        {
            // define shadow property
            builder.Entity<TEntity>().Property<bool>("IsDeleted");
        }

        public static void SetShadowProperties<TEntity>(ModelBuilder builder) where TEntity : class, IShadowProperties
        {
            // define shadow properties
            builder.Entity<TEntity>().Property<DateTime>("CreatedOn").IsRequired().HasDefaultValueSql("GetUtcDate()");
            builder.Entity<TEntity>().Property<DateTime?>("UpdatedOn").IsRequired(false);
            builder.Entity<TEntity>().Property<string>("CreatedBy").HasColumnType("VARCHAR").HasMaxLength(255).IsRequired();
            builder.Entity<TEntity>().Property<string>("UpdatedBy").HasColumnType("VARCHAR").HasMaxLength(255).IsRequired(false);
        }

        #endregion Shadow Properties

        #region Global Query Filters

        public static void SetGlobalQueryFilters(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var type = entityType.ClrType;

                // set global filters
                if (typeof(ISoftDelete).IsAssignableFrom(type))
                {
                    // softdeletable
                    SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(type)
                    .Invoke(modelBuilder, new object[] { modelBuilder });
                }
            }
        }

        private static readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo
            = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(SetGlobalQueryForSoftDelete), BindingFlags.Public | BindingFlags.Instance);

        public static void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

        #endregion Global Query Filters

        */
    }
}