using FacilityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class UpdateConfiguration : IEntityTypeConfiguration<Update>
{
    public void Configure(EntityTypeBuilder<Update> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Title).HasMaxLength(100);
        builder.Property(u => u.Message).HasMaxLength(255);
        builder.Property(u => u.CreatedById).HasMaxLength(450).IsRequired();
        builder.Property(u => u.AssociatedToId).HasMaxLength(450);

        builder.Property(u => u.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();
    }
}