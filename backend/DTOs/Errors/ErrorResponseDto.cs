namespace backend.DTOs;

public class ErrorResponoseDto
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}