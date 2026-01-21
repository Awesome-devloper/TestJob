using Microsoft.EntityFrameworkCore;
using TaskSchedulerLib.Domain.Entities;

namespace TaskSchedulerLib.Infrastructure.EF;

public class TaskDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }

    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }
}