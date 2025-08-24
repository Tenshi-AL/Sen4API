using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class PriorityConfiguration: IEntityTypeConfiguration<Priority>
{
    public void Configure(EntityTypeBuilder<Priority> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);

        builder.HasData(new Priority[]
        {
            new Priority()
            {
                Id = new Guid("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
                Name = "Low"
            },
            new Priority()
            {
                Id = Guid.NewGuid(),
                Name = "Medium"
            },
            new Priority()
            {
                Id = Guid.NewGuid(),
                Name = "Hight"
            },
        });
    }
}