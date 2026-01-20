using System.ComponentModel.DataAnnotations;

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
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExecutedAt { get; set; }
}