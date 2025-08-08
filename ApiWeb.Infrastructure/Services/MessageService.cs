using ApiWeb.Application.DTOs;
using ApiWeb.Application.Interfaces;
using ApiWeb.Domain.Entities;
using ApiWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _dbContext;
    private readonly IClientIdentityHasher _hasher;

    public MessageService(AppDbContext dbContext, IClientIdentityHasher hasher)
    {
        _dbContext = dbContext;
        _hasher = hasher;
    }

    public async Task<MessageDto> CreateAsync(string text, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var clientHash = _hasher.ComputeClientHash(ipAddress, userAgent);
        var message = new Message
        {
            Id = Guid.NewGuid(),
            Text = text.Trim(),
            CreatedAtUtc = now,
            ClientHash = clientHash,
            Upvotes = 0,
            Downvotes = 0
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(message);
    }

    public async Task<PagedResult<MessageDto>> GetFeedAsync(FeedQuery query, CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Messages.AsNoTracking();

        source = query.SortBy switch
        {
            SortBy.Top => source.OrderByDescending(x => x.Upvotes - x.Downvotes).ThenByDescending(x => x.CreatedAtUtc),
            _ => source.OrderByDescending(x => x.CreatedAtUtc)
        };

        var total = await source.CountAsync(cancellationToken);
        var skip = Math.Max(0, (query.Page - 1) * query.PageSize);
        var items = await source.Skip(skip).Take(query.PageSize)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<MessageDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<MessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Messages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return message is null ? null : MapToDto(message);
    }

    public async Task<int> VoteAsync(Guid messageId, int value, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        if (value != -1 && value != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Vote value must be -1 or 1");
        }

        var voterHash = _hasher.ComputeClientHash(ipAddress, userAgent);

        // Use a transaction to keep counts consistent
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var message = await _dbContext.Messages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
        if (message is null)
        {
            return 0;
        }

        var existingVote = await _dbContext.Votes.FirstOrDefaultAsync(x => x.MessageId == messageId && x.VoterHash == voterHash, cancellationToken);

        if (existingVote is null)
        {
            var vote = new Vote
            {
                MessageId = messageId,
                VoterHash = voterHash,
                Value = value,
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.Votes.Add(vote);

            if (value == 1) message.Upvotes += 1; else message.Downvotes += 1;
        }
        else if (existingVote.Value != value)
        {
            // Switch vote
            if (existingVote.Value == 1)
            {
                message.Upvotes -= 1;
                message.Downvotes += 1;
            }
            else
            {
                message.Downvotes -= 1;
                message.Upvotes += 1;
            }
            existingVote.Value = value;
        }
        // else: same vote again -> no change

        await _dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return message.Upvotes - message.Downvotes;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Messages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (message is null) return false;
        _dbContext.Messages.Remove(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static MessageDto MapToDto(Message x) => new()
    {
        Id = x.Id,
        Text = x.Text,
        CreatedAtUtc = x.CreatedAtUtc,
        Upvotes = x.Upvotes,
        Downvotes = x.Downvotes
    };
}