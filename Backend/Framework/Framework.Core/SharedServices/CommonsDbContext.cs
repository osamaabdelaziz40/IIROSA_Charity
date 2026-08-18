using Framework.Core.Data;
using Framework.Core.Helper;
using Framework.Core.SharedServices.Entities;
using Framework.Core.SharedServices.Seed;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Framework.Core.SharedServices
{
    /// <summary>
    ///     The commons db entities.
    /// </summary>
    public partial class CommonsDbContext : BaseDbContext<CommonsDbContext>, ICommonsDbContext
    {
        public CommonsDbContext(DbContextOptions<CommonsDbContext> options, IHttpContextAccessor httpContextAccessor, IIdentityTokenManager identityTokenManager)
            : base(options , httpContextAccessor , identityTokenManager)
        {
            CurrentUserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachment", "common");
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.DescriptionAr).HasMaxLength(500);
                entity.Property(e => e.DescriptionEn).HasMaxLength(500);
                entity.Property(e => e.Extension).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).HasMaxLength(255);
                entity.Property(e => e.TitleAr).HasMaxLength(255);
                entity.Property(e => e.TitleEn).HasMaxLength(255);

                //// Configure one-to-one relationship with AttachmentContent
                //entity.HasOne(a => a.AttachmentContent)
                //    .WithOne(ac => ac.Attachment)
                //    .HasForeignKey<AttachmentContent>(ac => ac.Id)
                //    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AttachmentType)
                 //.WithMany(p => p.Attachments)
                 //   .HasForeignKey(d => d.AttachmentTypeId)
                    ;

            });
            modelBuilder.Entity<AttachmentContent>(entity =>
            {
                entity.ToTable("AttachmentContent", "common");
                // Note: One-to-one relationship configured below on Attachment entity
            });

            // Configure one-to-one relationship: Attachment (principal) <-> AttachmentContent (dependent)
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.AttachmentContent)
                .WithOne(ac => ac.Attachment)
                .HasForeignKey<AttachmentContent>(ac => ac.AttachmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AttachmentType>(entity =>
            {
                entity.ToTable("AttachmentType", "common");
                entity.Property(e => e.AllowedFilesExtension).HasMaxLength(200);
                entity.Property(e => e.Code).IsRequired();
                entity.Property(e => e.MaxSizeInMegabytes).HasDefaultValueSql("((1))");
                entity.Property(e => e.FileNumber).HasDefaultValueSql("((1))");
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(256);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(256);
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Log", "common");
                entity.Property(e => e.Id);
                entity.Property(e => e.UserAgent).HasMaxLength(256).IsUnicode(false);
                entity.Property(e => e.Exception).HasMaxLength(6000).IsUnicode(false);
                //entity.Property(e => e.HttpMethod).HasMaxLength(256).IsUnicode(false);
                entity.Property(e => e.Host).HasMaxLength(256);
                entity.Property(e => e.LogLevel).IsRequired(false).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.Logger).IsRequired(false).HasMaxLength(256).IsUnicode(false);
                entity.Property(e => e.Message).IsRequired(false).HasMaxLength(4000).IsUnicode(false);
                entity.Property(e => e.Thread).IsRequired(false).HasMaxLength(256).IsUnicode(false);
                entity.Property(e => e.Url).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.UserName).HasMaxLength(256).IsUnicode(false);
            });

            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.ToTable("NotificationTemplate", "common");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BodyAr).IsRequired();
                entity.Property(e => e.BodyEn).IsRequired(false);
                entity.Property(e => e.SubjectAr).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SubjectEn).IsRequired(false).HasMaxLength(256);

                entity.HasOne(d => d.NotificationType)
                    .WithMany(p => p.NotificationTemplates)
                    .HasForeignKey(d => d.NotificationTypeId);
            });
            modelBuilder.Entity<NotificationType>(entity =>
            {
                entity.ToTable("NotificationType", "common");
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.ToTable("SystemSetting", "common");
                entity.Property(e => e.GroupName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.IsSticky).IsRequired();
                entity.Property(e => e.IsSecure).IsRequired();
                entity.Property(e => e.ValueType).IsRequired().HasMaxLength(30).IsUnicode(false);
            });
            modelBuilder.Entity<NotificationsLog>(entity =>
            {
                entity.ToTable("NotificationsLog", "common");
                entity.HasOne(d => d.NotificationType)
                    .WithMany(p => p.NotificationsLogs)
                    .HasForeignKey(d => d.NotificationTypeId);

            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLog", "common");
                entity.Property(e => e.AuditLogId).HasDefaultValueSql("newsequentialid()");
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.IpAddress).HasMaxLength(64);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.FieldChanges).HasColumnType("nvarchar(max)");
                entity.Property(e => e.AdditionalContext).HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserAgent).HasMaxLength(512);

                // Indexes for performance
                entity.HasIndex(e => e.EntityType).HasDatabaseName("IX_AuditLog_EntityType");
                entity.HasIndex(e => e.EntityId).HasDatabaseName("IX_AuditLog_EntityId");
                entity.HasIndex(e => e.Operation).HasDatabaseName("IX_AuditLog_Operation");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditLog_UserId");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLog_Timestamp");
                entity.HasIndex(e => e.CorrelationId).HasDatabaseName("IX_AuditLog_CorrelationId");
                entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp })
                    .HasDatabaseName("IX_AuditLog_Entity_EntityId_Timestamp");
            });

            modelBuilder.AddSeedData();
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<AttachmentContent> AttachmentContents { get; set; }
        public virtual DbSet<AttachmentType> AttachmentTypes { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public virtual DbSet<SystemSetting> SystemSettings { get; set; }
        public virtual DbSet<NotificationType> NotificationTypes { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        //public virtual DbSet<NotificationsLog> NotificationsLogs { get; set; }

    }
}