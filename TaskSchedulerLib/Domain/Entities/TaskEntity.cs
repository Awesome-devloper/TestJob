using System.ComponentModel.DataAnnotations;

namespace TaskSchedulerLib.Domain.Entities;

public class TaskEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TaskName { get; set; } = string.Empty;

    [Required]
    public string HandlerType { get; set; } = string.Empty;

    [Required]
    public string MethodName { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Queued, InProgress, Completed, Failed

    public int RetryCount { get; set; } = 0;

    public int MaxRetry { get; set; } = 10;

    public string? ErrorDetails { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime RetryUntil { get; set; } = DateTime.UtcNow.AddHours(1);
}