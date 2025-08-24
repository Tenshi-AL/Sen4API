using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class ProjectTaskConfiguration: IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.HasOne(p => p.UserCreated).WithMany(p => p.CreatedTasks);
        builder.HasOne(p => p.UserExecutor).WithMany(p => p.ResponsibilitiesTasks);
    }
}