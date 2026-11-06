using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class Department
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<TeacherProfile> Teachers { get; set; } = new List<TeacherProfile>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
}
