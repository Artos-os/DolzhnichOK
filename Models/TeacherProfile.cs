using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class TeacherProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<ResitSession> ResitSessions { get; set; } = new List<ResitSession>();
}
