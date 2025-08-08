namespace ApiWeb.Application.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }

    public int Score => Upvotes - Downvotes;
}