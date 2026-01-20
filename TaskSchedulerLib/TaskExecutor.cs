using Microsoft.EntityFrameworkCore;
using System.Reflection;

public class TaskExecutor : ITaskExecutor
{
    private readonly TaskDbContext _dbContext;

    public TaskExecutor(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteTask(int taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null) return;

        task.Status = "InProgress";
        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        try
        {
            // Parse HandlerType: "Namespace.Class, Assembly"
            string[] parts = task.HandlerType.Split(',');
            if (parts.Length != 2)
                throw new InvalidOperationException($"Invalid HandlerType format: {task.HandlerType}");

            string typeName = parts[0].Trim();
            string assemblyName = parts[1].Trim();

            Assembly assembly = Assembly.Load(assemblyName);
            var handlerType = assembly.GetType(typeName);
            if (handlerType == null)
                throw new InvalidOperationException($"Type {typeName} not found in assembly {assemblyName}.");

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
            task.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            task.ErrorDetails = ex.Message;
            task.RetryCount++;
            Console.WriteLine($"inner retry:{task.RetryCount}");
            if (DateTime.UtcNow > task.RetryUntil || task.RetryCount >= task.MaxRetry)
            {
                task.Status = "Failed";
            }
            else
            {
                task.Status = "Queued"; // Reset to Queued for retry
            }
            task.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            if (task.Status != "Failed")
            {
                throw; // Let Hangfire handle the retry
            }
        }
    }
}