using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.SqlServer;
using TaskSchedulerLib.Domain.Interfaces;
using TaskSchedulerLib.Application.Services;
using TaskSchedulerLib.Infrastructure.EF;
using TaskSchedulerLib.Infrastructure.Hangfire;

/// <summary>
/// Static class for configuring and initializing the Task Scheduler system.
/// Centralizes DI setup, DB configuration, Hangfire setup, and background job initialization.
/// </summary>
public static class TaskScheduler
{
    /// <summary>
    /// Configures services for the task scheduler, including DB context, Hangfire, and DI registrations.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure DB
        services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Configure Hangfire with SQL Server storage and automatic retries
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            config.UseFilter(new AutomaticRetryAttribute { Attempts = 10, DelaysInSeconds = new[] { 30 } });
        });
        services.AddHangfireServer();

        // Register services
        services.AddSingleton<IRunTask, TaskService>();
        services.AddScoped<ITaskExecutor, TaskExecutor>();
        services.AddScoped<PollingService>();
    }

    /// <summary>
    /// Initializes the system asynchronously: ensures DB is created and sets up recurring polling.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Ensure DB is created
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Start initial polling using service-based API for Hangfire
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<PollingService>("poll-job", ps => ps.Poll(), "* * * * * *");
    }
}