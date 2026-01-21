namespace TaskSchedulerLib.Domain.Interfaces;

public interface ITaskExecutor
{
    Task ExecuteTask(int taskId);
}