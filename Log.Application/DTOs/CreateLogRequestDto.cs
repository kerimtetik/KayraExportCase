namespace Log.Application.DTOs;

public class CreateLogRequestDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
}