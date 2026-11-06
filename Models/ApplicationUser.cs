using Microsoft.AspNetCore.Identity;

namespace DolzhnikOk.Models;

public sealed class ApplicationUser : IdentityUser<int>
{
    public bool IsActive { get; set; } = true;
    public StudentProfile? StudentProfile { get; set; }
    public TeacherProfile? TeacherProfile { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
