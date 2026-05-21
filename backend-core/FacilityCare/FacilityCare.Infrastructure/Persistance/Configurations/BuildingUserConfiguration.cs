using FacilityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class BuildingUserConfiguration : IEntityTypeConfiguration<BuildingUser>
{
    public void Configure(EntityTypeBuilder<BuildingUser> builder)
    {
        builder.HasKey(bu => new { bu.BuildingId, bu.UserId });

        builder.Property(bu => bu.UserId).HasMaxLength(450).IsRequired();
    }
}