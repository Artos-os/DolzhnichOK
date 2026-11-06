using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class StudentProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public int GroupId { get; set; }
    public StudentGroup Group { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
