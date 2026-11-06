namespace DolzhnikOk.Models;

public sealed class Booking
{
    public int Id { get; set; }
    public int ResitSessionId { get; set; }
    public ResitSession ResitSession { get; set; } = null!;
    public int StudentId { get; set; }
    public StudentProfile Student { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Active;
}

public enum BookingStatus
{
    Active,
    Cancelled
}
