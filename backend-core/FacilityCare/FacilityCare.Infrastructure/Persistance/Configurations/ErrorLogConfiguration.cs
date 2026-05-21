using FacilityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityCare.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Endpoint).HasMaxLength(255).IsRequired();
        builder.Property(e => e.ErrorMessage).IsRequired();
    }
}