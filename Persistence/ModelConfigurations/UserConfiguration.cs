using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        builder.Property(p => p.Surname).HasMaxLength(100);
        builder.Property(p => p.MiddleName).HasMaxLength(100);
    }
}