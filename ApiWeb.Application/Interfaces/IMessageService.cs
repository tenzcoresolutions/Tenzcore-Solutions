using ApiWeb.Application.DTOs;

namespace ApiWeb.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto> CreateAsync(string text, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task<PagedResult<MessageDto>> GetFeedAsync(FeedQuery query, CancellationToken cancellationToken = default);

    Task<MessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Returns new score after vote
    Task<int> VoteAsync(Guid messageId, int value, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}