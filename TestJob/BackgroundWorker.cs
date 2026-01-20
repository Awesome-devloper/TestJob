using Microsoft.EntityFrameworkCore;

public class BackgroundWorker
{
    private readonly TaskDbContext _dbContext;

    public BackgroundWorker(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var pendingTasks = await _dbContext.Tasks
                .Where(t => t.Status == "Pending")
                .ToListAsync(cancellationToken);

            foreach (var task in pendingTasks)
            {
                await ExecuteTaskAsync(task, cancellationToken);
            }

            await Task.Delay(1000, cancellationToken);
        }
    }

    private async Task ExecuteTaskAsync(TaskEntity task, CancellationToken cancellationToken)
    {
        task.Status = "Running";
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var handlerType = Type.GetType(task.HandlerType);
            if (handlerType == null)
                throw new InvalidOperationException($"Type {task.HandlerType} not found.");

            var instance = Activator.CreateInstance(handlerType);
            var method = handlerType.GetMethod(task.MethodName);
            if (method == null)
                throw new InvalidOperationException($"Method {task.MethodName} not found.");

            var result = method.Invoke(instance, null);
            if (result is Task taskResult)
            {
                await taskResult;
            }

            task.Status = "Completed";
            task.ExecutedAt = DateTime.UtcNow;
        }
        catch (Exception)
        {
            task.Status = "Failed";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}