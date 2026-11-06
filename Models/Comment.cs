using System.ComponentModel.DataAnnotations;

namespace DolzhnikOk.Models;

public sealed class Comment
{
    public int Id { get; set; }
    public int ResitSessionId { get; set; }
    public ResitSession ResitSession { get; set; } = null!;
    public int AuthorUserId { get; set; }
    public ApplicationUser Author { get; set; } = null!;

    [Required, StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
