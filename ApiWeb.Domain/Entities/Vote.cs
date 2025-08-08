namespace ApiWeb.Domain.Entities;

public class Vote
{
    public long Id { get; set; }

    public Guid MessageId { get; set; }

    public Message? Message { get; set; }

    public string VoterHash { get; set; } = string.Empty;

    // -1 for downvote, 1 for upvote
    public int Value { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}