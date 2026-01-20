using System.Linq.Expressions;

public class TaskService : IRunTask
{
    private readonly TaskDbContext _dbContext;

    public TaskService(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Invoke(Expression<Func<Task>> taskExpression)
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
            HandlerType = declaringType.FullName!,
            MethodName = method.Name
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();
    }
}