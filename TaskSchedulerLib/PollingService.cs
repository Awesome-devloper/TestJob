using Microsoft.EntityFrameworkCore;
using Hangfire;

public class PollingService
{
    private readonly TaskDbContext _dbContext;

    public PollingService(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Poll()
    {
        // Clean up expired tasks
        var expiredTasks = await _dbContext.Tasks
            .Where(t => t.Status == "Queued" && t.RetryUntil < DateTime.UtcNow && t.RetryCount >= t.MaxRetry)
            .ToListAsync();
         foreach (var task in expiredTasks)
        {
            task.Status = "Failed";
            task.ErrorDetails = "Retry time exceeded and max retries reached";
            task.UpdatedAt = DateTime.UtcNow;
        }
        if (expiredTasks.Any()) await _dbContext.SaveChangesAsync();

        var pendingTasks = await _dbContext.Tasks
            .Where(t => t.Status == "Pending" && t.RetryUntil > DateTime.UtcNow)
            .ToListAsync();

        foreach (var task in pendingTasks)
        {
            task.Status = "Queued";
            task.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue<ITaskExecutor>(te => te.ExecuteTask(task.Id));
        }
    }
}