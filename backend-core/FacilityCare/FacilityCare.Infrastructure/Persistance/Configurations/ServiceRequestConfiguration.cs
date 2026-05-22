using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sr => sr.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sr => sr.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sr => sr.PaymentIntentId).HasMaxLength(255);
        builder.Property(sr => sr.CreatedById).HasMaxLength(450).IsRequired();
        builder.Property(sr => sr.CustomerNotes).HasMaxLength(2000);

        builder.HasMany(sr => sr.Updates)
            .WithOne(u => u.ServiceRequest)
            .HasForeignKey(u => u.ServiceRequestId);
    }
}