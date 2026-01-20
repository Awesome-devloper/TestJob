using System.Linq.Expressions;

public interface IRunTask
{
    Task Invoke(Expression<Func<Task>> taskExpression);
}