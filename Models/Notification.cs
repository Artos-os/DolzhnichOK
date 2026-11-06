using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int? ResitSessionId { get; set; }
    public ResitSession? ResitSession { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
