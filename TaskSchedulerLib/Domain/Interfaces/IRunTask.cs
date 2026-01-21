using System.Linq.Expressions;

namespace TaskSchedulerLib.Domain.Interfaces;

/// <summary>
/// Interface for task invocation services.
/// </summary>
public interface IRunTask
{
    /// <summary>
    /// Invokes a task asynchronously with a specified maximum retry count.
    /// </summary>
    /// <param name="taskExpression">Expression representing the task method to invoke.</param>
    /// <param name="maxRetry">Maximum number of retries allowed (default 10).</param>
    Task Invoke(Expression<Func<Task>> taskExpression, int maxRetry = 10);
}