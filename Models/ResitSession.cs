using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class ResitSession
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public TeacherProfile Teacher { get; set; } = null!;
    public int ClassroomId { get; set; }
    public Classroom Classroom { get; set; } = null!;
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime? BookingDeadline { get; set; }
    public int Capacity { get; set; }
    public ResitSessionStatus Status { get; set; } = ResitSessionStatus.Open;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public enum ResitSessionStatus
{
    Open,
    Cancelled,
    Completed
}
