using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class ProjectConfiguration: IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        #if DEBUG
        builder.HasData(new Project[]
        {
            new Project()
            {
                Id = new Guid("587acc9e-26bc-43d7-9f1c-a7b0b59353c8"),
                Name = "Test Project",
                CreatedDateTime = DateTime.UtcNow,
            }
        });
        #endif
        
        builder.Property(p => p.Name).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(1000);


        builder.HasMany(p => p.Users)
            .WithMany(p => p.Projects)
            .UsingEntity<UsersProjects>();
    }
}