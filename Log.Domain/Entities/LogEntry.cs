namespace Log.Domain.Entities;

public class LogEntry
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}