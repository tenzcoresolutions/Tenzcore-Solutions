namespace ApiWeb.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class SubmitMessageRequest
{
    [Required]
    [MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    // Honeypot field for basic spam protection; clients should leave this empty
    public string? Honey { get; set; }
}