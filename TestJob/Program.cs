using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var builder = Host.CreateApplicationBuilder(args);

// Configure DB
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

// Register services
builder.Services.AddSingleton<BackgroundWorker>();
builder.Services.AddSingleton<IRunTask, TaskService>();
builder.Services.AddHostedService<BackgroundWorkerService>();

var host = builder.Build();

// Ensure DB is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Example usage
using (var scope = host.Services.CreateScope())
{
    var runTask = scope.ServiceProvider.GetRequiredService<IRunTask>();
    var doInstance = new Do();
    await runTask.Invoke(() => doInstance.DoSomething());
}

// Start the host
await host.RunAsync();

class Do
{
    public async Task DoSomething()
    {
        Console.WriteLine("123");
        await Task.CompletedTask;
    }
}
