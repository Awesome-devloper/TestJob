# TaskScheduler

A lightweight, database-driven background task execution system built with .NET 10, using Hangfire for retry handling and SQL Server for persistence. This system allows scheduling and executing asynchronous tasks with configurable retry logic, suitable for distributed environments.

## Features

- **Database-Driven**: Tasks are stored in SQL Server with full state management (Pending, InProgress, Completed, Failed).
- **Hangfire Integration**: Automatic retries with configurable attempts and delays (only for retries, not scheduling).
- **Time-Based Retries**: Tasks can retry for a specified duration (e.g., 1 hour) or up to a max count.
- **Multi-Instance Support**: Safe for distributed execution across multiple app instances.
- **Library Architecture**: Core logic in `TaskSchedulerLib` for reusability.
- **Docker Ready**: Includes setup for SQL Server via Docker.

## Requirements

- .NET 10 SDK
- Docker (for SQL Server)
- SQL Server (via Docker or local instance)

## Installation and Setup

### 1. Clone the Repository
```bash
git clone https://github.com/Awesome-devloper/TestJob.git
cd TestJob
```

### 2. Set Up SQL Server with Docker
Run the following command to start a SQL Server container:
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver --hostname sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```
- Replace `YourStrong!Passw0rd` with a strong password.
- The database `JobSchedulerDb` will be created automatically.

### 3. Build the Solution
```bash
dotnet build TestJobSolution.sln
```

### 4. Run the Application
```bash
cd TestJob
dotnet run
```
- The app will create the database schema, register a sample task, and start polling for execution.
- Sample output: Task fails 10 times, then succeeds on the 11th attempt, printing "123".

## Usage

### Basic Task Invocation
In your code, inject `IRunTask` and invoke tasks:
```csharp
var runTask = serviceProvider.GetRequiredService<IRunTask>();
await runTask.Invoke(() => MyTaskAsync(), maxRetry: 5);
```

### Configuration
- Update the connection string in `Program.cs` for your SQL Server instance.
- Adjust retry settings in `TaskScheduler.ConfigureServices()`.

### Sample Task Class
```csharp
class MyTask
{
    public async Task MyTaskAsync()
    {
        // Your task logic here
        await Task.Delay(1000);
        Console.WriteLine("Task completed!");
    }
}
```

## Architecture

- **TaskSchedulerLib**: Contains all business logic.
  - `IRunTask` & `TaskService`: Task registration.
  - `TaskEntity` & `TaskDbContext`: Database models.
  - `ITaskExecutor` & `TaskExecutor`: Task execution with retries.
  - `PollingService`: Background polling for pending tasks.
  - `TaskScheduler`: Static class for DI setup and initialization.
- **TestJob**: Main console app for configuration and example usage.
- **Hangfire**: Handles job queuing and retries (not scheduling).
- **SQL Server**: Stores tasks, Hangfire jobs, and state.

## API Reference

### IRunTask
- `Task Invoke(Expression<Func<Task>> taskExpression, int maxRetry = 10)`: Registers and invokes a task.

### TaskScheduler (Static)
- `ConfigureServices(IServiceCollection, IConfiguration)`: Sets up DI.
- `InitializeAsync(IServiceProvider)`: Initializes DB and polling.

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Commit changes and push.
4. Submit a pull request.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Support

For issues or questions, open a GitHub issue or contact the maintainer.