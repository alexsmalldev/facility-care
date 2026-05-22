using FacilityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.Name).HasMaxLength(255).IsRequired();
        builder.Property(st => st.Description).IsRequired();
        builder.Property(st => st.ServiceIcon).HasMaxLength(500);
        builder.Property(st => st.IsPaid).IsRequired();
        builder.Property(st => st.Price).HasPrecision(10, 2);

        builder.HasMany(st => st.ServiceRequests)
            .WithOne(sr => sr.ServiceType)
            .HasForeignKey(sr => sr.ServiceTypeId);
    }
}