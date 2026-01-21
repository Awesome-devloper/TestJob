using System.Linq.Expressions;
using TaskSchedulerLib.Domain.Entities;
using TaskSchedulerLib.Domain.Interfaces;
using TaskSchedulerLib.Infrastructure.EF;

namespace TaskSchedulerLib.Application.Services;

/// <summary>
/// Service for registering tasks in the database.
/// Parses expressions to extract type and method information for later execution.
/// </summary>
public class TaskService : IRunTask
{
    private readonly TaskDbContext _dbContext;

    public TaskService(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Registers a task by parsing the expression and storing metadata in the database.
    /// </summary>
    /// <param name="taskExpression">Expression of the task method.</param>
    /// <param name="maxRetry">Max retries for the task.</param>
    public async Task Invoke(Expression<Func<Task>> taskExpression, int maxRetry = 10)
    {
        if (taskExpression.Body is not MethodCallExpression methodCall)
            throw new ArgumentException("Expression must be a method call.");

        var method = methodCall.Method;
        var declaringType = method.DeclaringType;
        if (declaringType == null)
            throw new InvalidOperationException("Method must be declared in a type.");

        var task = new TaskEntity
        {
            TaskName = method.Name,
            HandlerType = declaringType.FullName + ", " + declaringType.Assembly.GetName().Name,
            MethodName = method.Name,
            MaxRetry = maxRetry,
            RetryUntil = DateTime.UtcNow.AddHours(1) // Retry for 1 hour
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();
    }
}