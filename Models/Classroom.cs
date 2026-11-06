using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class Classroom
{
    public int Id { get; set; }

    [Required, StringLength(10)]
    public string Number { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<ResitSession> ResitSessions { get; set; } = new List<ResitSession>();
}
