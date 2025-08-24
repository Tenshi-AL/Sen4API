using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class TaskStatusConfiguration: IEntityTypeConfiguration<Domain.Models.TaskStatus>
{
    public void Configure(EntityTypeBuilder<Domain.Models.TaskStatus> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        
        builder.HasData(new Domain.Models.TaskStatus[]
        {
        #if DEBUG
            new Domain.Models.TaskStatus()
            {
                Id = new Guid("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
                Name = "Test"
            },
        #endif
        });
    }
}