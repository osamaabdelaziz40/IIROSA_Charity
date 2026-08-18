//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using IIROSA.Domain.Entities;

//namespace IIROSA.Domain.Configurations;

///// <summary>
///// Entity Framework Configuration for ImpersonationSession entity
///// </summary>
//public class ImpersonationSessionConfiguration : IEntityTypeConfiguration<ImpersonationSession>
//{
//    public void Configure(EntityTypeBuilder<ImpersonationSession> builder)
//    {
//        builder.ToTable(nameof(ImpersonationSession), MappingDefaults.IIROSA_SCHEMA);

//        builder.HasKey(x => x.Id);

//        // ========== Basic Information ==========
//        builder.Property(x => x.ImpersonatorUserId)
//            .IsRequired();

//        builder.Property(x => x.ImpersonatorUserName)
//            .HasMaxLength(256);

//        builder.Property(x => x.ImpersonatorEmail)
//            .HasMaxLength(256);

//        builder.Property(x => x.ImpersonatedUserId)
//            .IsRequired();

//        builder.Property(x => x.ImpersonatedUserName)
//            .HasMaxLength(256);

//        builder.Property(x => x.ImpersonatedEmail)
//            .HasMaxLength(256);

//        builder.Property(x => x.StartTime)
//            .IsRequired();

//        builder.Property(x => x.EndTime);

//        builder.Property(x => x.IsActive)
//            .IsRequired();

//        builder.Property(x => x.OriginalUserIpAddress)
//            .HasMaxLength(50);

//        builder.Property(x => x.UserAgent)
//            .HasMaxLength(500);

//        builder.Property(x => x.ActionsPerformedCount)
//            .HasDefaultValue(0);

//        builder.Property(x => x.TerminatedByName)
//            .HasMaxLength(256);

//        builder.Property(x => x.TerminationReason)
//            .HasMaxLength(200);

//        // ========== Relationships ==========
//        // Note: Navigation to AppIdentityUser requires relationship configuration
//        // These are relationships with entities from Framework.Identity

//        // ========== Indexes ==========
//        builder.HasIndex(x => x.ImpersonatorUserId);
//        builder.HasIndex(x => x.ImpersonatedUserId);
//        builder.HasIndex(x => x.IsActive);
//        builder.HasIndex(x => x.StartTime);
//        builder.HasIndex(x => x.EndTime);

//        // Composite index for active sessions query
//        builder.HasIndex(x => new { x.IsActive, x.StartTime })
//            .IsDescending(false, true);

//        // Composite index for user's active sessions
//        builder.HasIndex(x => new { x.ImpersonatorUserId, x.IsActive });
//    }
//}
