using FacilityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.AddressLine1).HasMaxLength(255).IsRequired();
        builder.Property(b => b.AddressLine2).HasMaxLength(255);
        builder.Property(b => b.City).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Postcode).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Country).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Latitude).HasPrecision(9, 6);
        builder.Property(b => b.Longitude).HasPrecision(9, 6);

        builder.HasMany(b => b.BuildingUsers)
            .WithOne(bu => bu.Building)
            .HasForeignKey(bu => bu.BuildingId);

        builder.HasMany(b => b.ServiceRequests)
            .WithOne(sr => sr.Building)
            .HasForeignKey(sr => sr.BuildingId);
    }
}