using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class Subject
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<ResitSession> ResitSessions { get; set; } = new List<ResitSession>();
}
