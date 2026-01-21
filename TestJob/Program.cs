using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskSchedulerLib;
using TaskSchedulerLib.Domain.Interfaces;

/// <summary>
/// Entry point for the Task Scheduler application.
/// This console app demonstrates a database-driven background task execution system
/// with Hangfire for retries, using a separate library for core logic.
/// </summary>
var builder = Host.CreateApplicationBuilder(args);

// Add configuration for connection strings
builder.Configuration.AddInMemoryCollection(new[]
{
    new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", "Server=localhost,1433;Database=JobSchedulerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;")
});

// Configure services using the library
TaskScheduler.ConfigureServices(builder.Services, builder.Configuration);

var host = builder.Build();

// Initialize using the library (DB creation and polling setup)
await TaskScheduler.InitializeAsync(host.Services);

// Example usage: Register and execute a task with retries
using (var scope = host.Services.CreateScope())
{
    var runTask = scope.ServiceProvider.GetRequiredService<IRunTask>();
    var doInstance = new Do();
    await runTask.Invoke(() => doInstance.DoSomething(), 1); // Example with 1 max retry for demo
}

// Start the host (runs Hangfire server and background services)
await host.RunAsync();

/// <summary>
/// Sample task class demonstrating retry behavior.
/// Simulates failures for the first 10 attempts, then succeeds.
/// </summary>
class Do
{
    public static int Retry { get; set; } = 0;

    /// <summary>
    /// Asynchronous task method that may fail multiple times before succeeding.
    /// </summary>
    public async Task DoSomething()
    {
        if (Retry < 11)
        {
            Retry++;
            Console.WriteLine($"Attempt {Retry}");
            throw new Exception("Simulated failure");
        }
        Console.WriteLine("123");
        await Task.CompletedTask;
    }
}
