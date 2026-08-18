using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(nameof(Employee), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NationalId)
            .HasMaxLength(50);

        builder.Property(x => x.Gender)
            .HasMaxLength(10);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Position)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.Email);
    }
}
