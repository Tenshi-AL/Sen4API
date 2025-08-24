using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.ModelConfigurations;

public class PostConfiguration: IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(100);

        builder.HasData(new Post[]
        {
            #if DEBUG
            new Post()
            {
                Id = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
                Title = "test post"
            },
            #endif
        });
    }
}