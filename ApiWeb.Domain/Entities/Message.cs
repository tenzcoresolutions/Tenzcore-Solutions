namespace ApiWeb.Domain.Entities;

using System.ComponentModel.DataAnnotations;

public class Message
{
    public Guid Id { get; set; }

    [MaxLength(300)]
    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string? ClientHash { get; set; }

    public int Upvotes { get; set; }

    public int Downvotes { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}