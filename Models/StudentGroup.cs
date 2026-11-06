using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class StudentGroup
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
}
