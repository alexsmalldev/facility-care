namespace FacilityCare.Domain.Entities;

public class ErrorLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Endpoint { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}