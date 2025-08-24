using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.ModelConfigurations;
using TaskStatus = Domain.Models.TaskStatus;

namespace Persistence;

public class Sen4Context: IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public Sen4Context(DbContextOptions<Sen4Context> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public Sen4Context()
    {
        Database.EnsureCreated();
    }
    
    public DbSet<Post> Posts { get; set; }
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<TaskFile> TaskFiles { get; set; }
    public DbSet<TaskStatus> TaskStatuses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<UsersProjects> UsersProjects { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new PostConfiguration());
        builder.ApplyConfiguration(new PriorityConfiguration());
        builder.ApplyConfiguration(new ProjectConfiguration());
        builder.ApplyConfiguration(new ProjectTaskConfiguration());
        builder.ApplyConfiguration(new TaskStatusConfiguration());
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new OperationConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}